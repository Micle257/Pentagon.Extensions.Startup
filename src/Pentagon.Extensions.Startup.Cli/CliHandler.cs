namespace Pentagon.Extensions.Startup.Cli
{
    using System;
    using System.Collections.Generic;
    using System.CommandLine;
    using System.CommandLine.Binding;
    using System.CommandLine.Invocation;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;

    public abstract class CliHandler<TOptions> : ICliHandler<TOptions>
    {
        readonly CancellationTokenSource _cancellationToken;

        protected CliHandler()
        {
            var s = DICore.Get<IProgramCancellationSource>();

            _cancellationToken = s?.CancellationTokenSource;
        }

        /// <inheritdoc />
        public abstract Task<int> RunAsync(TOptions options, CancellationToken cancellationToken = default);

        /// <inheritdoc />
        public Task<int> InvokeAsync(InvocationContext context)
        {
            var bindingContext = context.BindingContext;

            var type = typeof(TOptions);

            var options = new ReflectionModelBinder<TOptions>().CreateInstance(bindingContext);

            return RunAsync(options, _cancellationToken?.Token ?? CancellationToken.None);
        }
    }

    public class ReflectionModelBinder<T>
    {
        public T CreateInstance(BindingContext bindingContext)
        {
            var commandResult = bindingContext.ParseResult.CommandResult;

            var meta = CommandHelper.GetHierarchy().FirstOrDefault(a => a.Value.Type == typeof(T)).Value;

            var options = meta.Options;
            var argument = meta.Command.Argument;

            var result = Activator.CreateInstance<T>();

            foreach (var optionMetadata in options)
            {
                var symbolResult = commandResult.Children.FirstOrDefault(a => a.HasAlias(optionMetadata.Attribute.Aliases.FirstOrDefault()));

                if (symbolResult != null)
                {
                    if (symbolResult.ArgumentResult is SuccessfulArgumentResult<bool> boolRes)
                    {
                        optionMetadata.PropertyInfo.SetValue(result, boolRes.Value);
                    }
                    else if (symbolResult.ArgumentResult is SuccessfulArgumentResult<object> res)
                        optionMetadata.PropertyInfo.SetValue(result, res.Value);
                    else
                    {
                        
                    }
                }
            }

            for (var i = 0; i < commandResult.Arguments.Count; i++)
            {
                var cmd = commandResult.Arguments.ElementAt(i);

                var cliCommnand = meta.Arguments[i];

                cliCommnand.PropertyInfo.SetValue(result, cmd);
            }

            return result;
        }
    }
}