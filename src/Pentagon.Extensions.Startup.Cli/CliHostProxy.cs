namespace Pentagon.Extensions.Startup.Cli {
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    class CliHostProxy : IHost
    {
        readonly IHost _host;

        public CliHostProxy(IHost host)
        {
            _host = host;
        }

        /// <inheritdoc />
        public void Dispose() => _host.Dispose();

        /// <inheritdoc />
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var runner = Services.GetRequiredService<ICliHostedService>();
            //var applicationLifetime = Services.GetRequiredService<IHostApplicationLifetime>();
            //var hostLifetime = Services.GetRequiredService<IHostLifetime>();

            //using var combinedCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, applicationLifetime.ApplicationStopping);
            //var combinedCancellationToken = combinedCancellationTokenSource.Token;

            // TODO delay
            //await hostLifetime.WaitForStartAsync(combinedCancellationToken).ConfigureAwait(false);

            //combinedCancellationToken.ThrowIfCancellationRequested();

            await runner.ExecuteAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public IServiceProvider Services => _host.Services;
    }
}