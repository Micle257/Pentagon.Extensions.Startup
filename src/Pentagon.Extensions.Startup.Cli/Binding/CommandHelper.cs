namespace Pentagon.Extensions.Startup.Cli {
    using System;
    using System.Collections.Generic;
    using System.CommandLine;
    using System.Linq;
    using System.Reflection;
    using Collections;

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
                            .Where(a => a.GetCustomAttribute<CommandAttribute>() != null)
                            .Select(a => new CommandMetadata
                                         {
                                                 Type = a,
                                                 Attribute = a.GetCustomAttribute<CommandAttribute>(),
                                                 Command = GetCommand(a, a.GetCustomAttribute<CommandAttribute>())
                                         } )
                            .ToArray();
        }

        static Command GetCommand(Type type, CommandAttribute attribute)
        {
            var options = type.GetProperties()
                              .Where(a => a.GetCustomAttribute<OptionsAttribute>() != null)
                              .Select(a => (a, a.GetCustomAttribute<OptionsAttribute>()))
                              .ToArray();

            var command = new Command(attribute.Name);

            foreach (var (propertyInfo, optionsAttribute) in options)
            {
                command.AddOption( new Option(optionsAttribute.Aliases, argument: new Argument
                                                                                  {
                                                                                          ArgumentType = propertyInfo.PropertyType
                                                                                  }));
            }

            try { command.AddHandler(type); }
            catch 
            {
            }
            
            return command;
        }
    }
}