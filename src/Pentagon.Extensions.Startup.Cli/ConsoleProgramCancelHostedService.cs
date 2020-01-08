namespace Pentagon.Extensions.Startup.Cli {
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using JetBrains.Annotations;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    public class ConsoleProgramCancelHostedService : BackgroundService
    {
        readonly ILogger<ConsoleProgramCancelHostedService> _logger;

        [NotNull]
        readonly IHostApplicationLifetime _applicationLifetime;

        [NotNull]
        ConsoleProgramCancellationOptions _options;

        public ConsoleProgramCancelHostedService(ILogger<ConsoleProgramCancelHostedService> logger,
                                                 [NotNull] IHostApplicationLifetime applicationLifetime,
                                                 IOptions<ConsoleProgramCancellationOptions> options)
        {
            _logger              = logger;
            _applicationLifetime = applicationLifetime;
            _options             = options?.Value ?? new ConsoleProgramCancellationOptions();
        }

        /// <inheritdoc />
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (_options.KeyPredicate == null)
            {
                _logger.LogDebug("Cancel key handler is null.");
                return;
            }

            if (!Environment.UserInteractive)
            {
                _logger.LogDebug("Cancel key handler cannot execute: console handle is not present.");
                return;
            }

            do
            {
                ConsoleKeyInfo read;

                if (Console.KeyAvailable)
                {
                    read = Console.ReadKey(true);
                }

                if (_options.KeyPredicate(read))
                {
                    _applicationLifetime.StopApplication();
                    _logger.LogInformation("Cancel key handler: cancel requested.");
                }

                await Task.Delay(100, stoppingToken).ConfigureAwait(false);
            } while (true);
        }
    }
}