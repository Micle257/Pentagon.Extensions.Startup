namespace Pentagon.Extensions.Startup.Cli {
    using Microsoft.Extensions.Options;

    public interface ICliOptionsSource<TOptions> : IOptionsChangeTokenSource<TOptions>
    {
        void Reload();

        TOptions Options { get; }
    }
}