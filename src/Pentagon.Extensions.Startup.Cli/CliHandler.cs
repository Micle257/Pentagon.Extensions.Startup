namespace Pentagon.Extensions.Startup.Cli
{
    using System.Collections.Generic;
    using System.CommandLine.Binding;
    using System.CommandLine.Invocation;
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

            var options = (TOptions)new ModelBinder(type).CreateInstance(bindingContext);

            return RunAsync(options, _cancellationToken?.Token ?? CancellationToken.None);
        }
    }
}