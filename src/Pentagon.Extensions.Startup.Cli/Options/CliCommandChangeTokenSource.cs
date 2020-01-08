namespace Pentagon.Extensions.Startup.Cli {
    using System.Threading;
    using Microsoft.Extensions.Primitives;

    public class CliCommandChangeTokenSource<TOptions> : ICliOptionsSource<TOptions>
            where TOptions : new()
    {
        private CliCommandReloadToken _token;

        public CliCommandChangeTokenSource(string name = null)
        {
            _token = new CliCommandReloadToken();

            Name = name ?? Microsoft.Extensions.Options.Options.DefaultName;
            Options = new TOptions();
        }

        public string Name { get; }

        public IChangeToken GetChangeToken()
        {
            return _token;
        }

        public void Reload()
        {
            var previousToken = Interlocked.Exchange(ref _token, new CliCommandReloadToken());
            previousToken.OnReload();
        }

        /// <inheritdoc />
        public TOptions Options { get; }
    }
}