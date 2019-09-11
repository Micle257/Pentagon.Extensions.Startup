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
    using JetBrains.Annotations;
    using Logging;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public abstract class CliHandler<TOptions> : ICliHandler<TOptions>
    {
        readonly CancellationToken _cancellationToken;

        protected CliHandler()
        {
            var s = DICore.Get<IProgramCancellationSource>();

            _cancellationToken = s?.Token ?? CancellationToken.None;
        }

        /// <inheritdoc />
        public abstract Task<int> RunAsync(TOptions options, CancellationToken cancellationToken);

        protected virtual Task OnCancelAsync()
        {
            var logger = DICore.App?.Services?.GetService<ILogger<CliHandler<TOptions>>>();

            logger?.LogDebug("Command was cancelled: {TypeName}.", GetType().Name);

            return Task.CompletedTask;
        }

        protected virtual Task OnErrorAsync(Exception e)
        {
            var logger = DICore.App?.Services?.GetService<ILogger<CliHandler<TOptions>>>();

            logger?.LogError(e,"Command execution failed: {TypeName}. {ExceptionMessage}", GetType().Name, e.Message);

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        [NotNull]
        public async Task<int> InvokeAsync([NotNull] InvocationContext context)
        {
            var bindingContext = context.BindingContext;

            var type = typeof(TOptions);

            var options = new ReflectionModelBinder<TOptions>().CreateInstance(bindingContext);

            try
            {
                return await RunAsync(options, _cancellationToken);
            }
            catch (OperationCanceledException)
            {
                await OnCancelAsync();
                return StatusCodes.Cancel;
            }
            catch (Exception e)
            {
                await OnErrorAsync(e);
                return StatusCodes.Error;
            }
        }
    }

    public class ReflectionModelBinder<T>
    {
        public T CreateInstance([NotNull] BindingContext bindingContext)
        {
            if (bindingContext == null)
                throw new ArgumentNullException(nameof(bindingContext));

            var commandResult = bindingContext.ParseResult?.CommandResult;

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