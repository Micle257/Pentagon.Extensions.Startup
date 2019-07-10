namespace Pentagon.Extensions.Startup.Cli
{
    using System;
    using System.Collections.Generic;
    using System.CommandLine;
    using System.CommandLine.Binding;
    using System.ComponentModel;
    using System.Linq;
    using System.Reflection;
    using Collections;
    using JetBrains.Annotations;

    public static class CommandHelper
    {
        public static RootCommand GetRootCommand()
        {
            var h = GetHierarchy();

            return h.Root.Value.Command as RootCommand;
        }

        public static HierarchyList<CommandMetadata> GetHierarchy()
        {
            var users = GetMetadata();

            // resolve self parent users
            foreach (var user in users.Where(a => a.Attribute.Parent == a.Type))
            {
                user.Attribute.Parent = null;
            }

            // resolve looped users
            foreach (var user in users.Where(a => a.Attribute.Parent != null))
            {
                var user2 = users.FirstOrDefault(a => a.Attribute.Parent == user.Type && user.Attribute.Parent == a.Type);

                if (user2 == null)
                    continue;

                user.Attribute.Parent = null;
                user2.Attribute.Parent = null;
            }

            var rootCommand = new CommandMetadata
            {
                Command = new RootCommand()
            };

            var result = new HierarchyList<CommandMetadata>(rootCommand);

            InitializeHierarchyItem(result.Root);

            return result;

            void InitializeHierarchyItem(HierarchyListNode<CommandMetadata> item)
            {
                var children = users.Where(a => a.Attribute.Parent == item.Value.Type).ToList();

                if (!children.Any())
                    return;

                foreach (var child in children)
                {
                    item.AddChildren(child);

                    item.Value.Command.AddCommand(child.Command);

                    InitializeHierarchyItem(item.Children.FirstOrDefault(a => a.Value == child));
                }
            }
        }

        static IEnumerable<CommandMetadata> GetMetadata()
        {
            return AppDomain.CurrentDomain
                            .GetAssemblies()
                            .SelectMany(a => a.GetTypes())
                            .Where(a => a.GetCustomAttribute<CliCommandAttribute>() != null)
                            .Select(a => new CommandMetadata
                            {
                                Type = a,
                                Attribute = a.GetCustomAttribute<CliCommandAttribute>(),
                                Command = GetCommand(a, a.GetCustomAttribute<CliCommandAttribute>())
                            })
                            .Select(a =>
                                    {
                                        a.Options = GetOptions(a);
                                        a.Arguments = GetArguments(a);
                                        a.Command = GetCommand(a.Type, a.Attribute);
                                        return a;
                                    })
                            .ToArray();
        }

        static IReadOnlyList<OptionMetadata> GetOptions(CommandMetadata commandMetadata)
        {
            var options = commandMetadata.Type.GetProperties()
                                         .Where(a => a.GetCustomAttribute<CliOptionAttribute>() != null)
                                         .Select(a => (a, a.GetCustomAttribute<CliOptionAttribute>()));

            return options.Select(a => new OptionMetadata
            {
                Attribute = a.Item2,
                PropertyInfo = a.a
            }).ToList();
        }

        static IReadOnlyList<ArgumentMetadata> GetArguments(CommandMetadata commandMetadata)
        {
            var options = commandMetadata.Type.GetProperties()
                                         .Where(a => a.GetCustomAttribute<CliArgumentAttribute>() != null)
                                         .Select(a => (a, a.GetCustomAttribute<CliArgumentAttribute>()))
                                         .OrderBy(a => a.Item2.Index);

            return options.Select(a => new ArgumentMetadata
            {
                                               Attribute = a.Item2,
                                               PropertyInfo = a.a
                                       }).ToList();
        }

        static Command GetCommand(Type type, CliCommandAttribute attribute)
        {
            var command = new Command(attribute.Name);

            ApplyOptions(type, command);
            ApplyArguments(type, command);

            try { command.AddHandler(type); }
            catch
            {
            }

            return command;
        }

        static void ApplyArguments(Type type, Command command)
        {
            var arguments = type.GetProperties()
                              .Where(a => a.GetCustomAttribute<CliArgumentAttribute>() != null)
                              .Select(a => (a, a.GetCustomAttribute<CliArgumentAttribute>()))
                              .ToArray();

            var requiredCount = arguments.Select(a => a.Item2).Count(a => a.IsRequired);

            command.Argument = new Argument { Arity = new ArgumentArity(requiredCount, arguments.Length) };

            foreach (var (propertyInfo, argumentAttribute) in arguments)
            {

            }
        }

        static void ApplyOptions(Type type, Command command)
        {
            var options = type.GetProperties()
                              .Where(a => a.GetCustomAttribute<CliOptionAttribute>() != null)
                              .Select(a => (a, a.GetCustomAttribute<CliOptionAttribute>()))
                              .ToArray();

            foreach (var (propertyInfo, optionsAttribute) in options)
            {
                var propType = propertyInfo.PropertyType;

                var supportsEmpty = IsNullable(propType);

                propType = GetItemTypeIfNullable(propType) ?? propType;

                var arg = new Argument
                          {
                                  ArgumentType = propType
                          };

                if (supportsEmpty)
                    arg.Arity = new ArgumentArity(0, 1);

                if (optionsAttribute.IsRequired)
                    arg.Arity = new ArgumentArity(1, 1);

                command.AddOption(new Option(optionsAttribute.Aliases,
                                             description: optionsAttribute.Description ?? string.Empty,
                                             argument: arg));
            }
        }

        static bool IsNullable(Type type)
        {
            if (!type.IsValueType) return true;

            return Nullable.GetUnderlyingType(type) != null;
        }

        static bool IsTypeSupported([NotNull] Type type, bool isNested = false)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (type.IsPrimitive || type.IsEnum)
            {
                return true;
            }

            if (type == typeof(string))
            {
                return true;
            }

            if (TypeDescriptor.GetConverter(type) is TypeConverter typeConverter &&
                typeConverter.CanConvertFrom(typeof(string)))
            {
                return true;
            }

            if (isNested)
                return false;

            if (TryFindConstructorWithSingleParameterOfType(type, typeof(string)))
            {
                return true;
            }

            if (GetItemTypeIfNullable(type) is Type coll)
            {
                return IsTypeSupported(coll, true);
            }

            if (GetItemTypeIfEnumerable(type) is Type nullable)
            {
                return IsTypeSupported(nullable, true);
            }

            return false;
        }

        static bool TryFindConstructorWithSingleParameterOfType(
                Type type,
                Type parameterType)
        {
            var (x, y) = type.GetConstructors()
                             .Select(c => (ctor: c, parameters: c.GetParameters()))
                             .SingleOrDefault(tuple => tuple.ctor.IsPublic &&
                                                       tuple.parameters.Length == 1 &&
                                                       tuple.parameters[0].ParameterType == parameterType);

            return x != null;
        }

        static Type GetNullable([NotNull] Type obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            if (obj.IsGenericType && obj.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                return  obj.GetGenericArguments()[0];
            }

            return obj;
        }

        static Type GetItemTypeIfNullable([NotNull] Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return  type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) ? Nullable.GetUnderlyingType(type) : null;
        }

        private static Type GetItemTypeIfEnumerable([NotNull] Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            var enumerableInterface = IsEnumerable(type)
                                              ? type
                                                : type
                                                  .GetInterfaces()
                                                  .FirstOrDefault(IsEnumerable);

            if (enumerableInterface == null)
            {
                return null;
            }

            return enumerableInterface.GenericTypeArguments[0];

            bool IsEnumerable(Type arg) => arg.IsGenericType && arg.GetGenericTypeDefinition() == typeof(IEnumerable<>);
        }
    }
}