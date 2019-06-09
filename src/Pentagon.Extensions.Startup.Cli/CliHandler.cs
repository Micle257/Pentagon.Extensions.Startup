namespace Pentagon.Extensions.Startup.Cli {
    using System.Threading;
    using System.Threading.Tasks;

    public abstract class CliHandler<TOptions> : ICliHandler<TOptions>
    {
        public virtual void BuildApp(TOptions options) { }

        /// <inheritdoc />
        public abstract Task<int> RunAsync(TOptions options, CancellationToken cancellationToken = default);
    }
}