// -----------------------------------------------------------------------
//  <copyright file="HostBuilderExtensions.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Startup.Cli
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Console.Cli;
    using JetBrains.Annotations;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    public class CliHostProxy : IHost
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

    public class CliHostBuilderProxy : IHostBuilder
    {
        readonly IHostBuilder _hostBuilder;

        public CliHostBuilderProxy(IHostBuilder hostBuilder)
        {
            _hostBuilder = hostBuilder;
        }

        /// <inheritdoc />
        public IHostBuilder ConfigureHostConfiguration(Action<IConfigurationBuilder> configureDelegate)
        {
            _hostBuilder.ConfigureHostConfiguration(configureDelegate);

            return this;
        }

        /// <inheritdoc />
        public IHostBuilder ConfigureAppConfiguration(Action<HostBuilderContext, IConfigurationBuilder> configureDelegate)
        {
            _hostBuilder.ConfigureAppConfiguration(configureDelegate);

            return this;
        }

        /// <inheritdoc />
        public IHostBuilder ConfigureServices(Action<HostBuilderContext, IServiceCollection> configureDelegate)
        {
            _hostBuilder.ConfigureServices(configureDelegate);

            return this;
        }

        /// <inheritdoc />
        public IHostBuilder UseServiceProviderFactory<TContainerBuilder>(IServiceProviderFactory<TContainerBuilder> factory)
        {
            _hostBuilder.UseServiceProviderFactory(factory);

            return this;
        }

        /// <inheritdoc />
        public IHostBuilder UseServiceProviderFactory<TContainerBuilder>(Func<HostBuilderContext, IServiceProviderFactory<TContainerBuilder>> factory)
        {
            _hostBuilder.UseServiceProviderFactory(factory);

            return this;
        }

        /// <inheritdoc />
        public IHostBuilder ConfigureContainer<TContainerBuilder>(Action<HostBuilderContext, TContainerBuilder> configureDelegate)
        {
            _hostBuilder.ConfigureContainer(configureDelegate);

            return this;
        }

        /// <inheritdoc />
        public IHost Build()
        {
            var host = _hostBuilder.Build();

            return new CliHostProxy(host);
        }

        /// <inheritdoc />
        public IDictionary<object, object> Properties => _hostBuilder.Properties;
    }

    public static class HostBuilderExtensions
    {
        [NotNull]
        public static IHostBuilder UseCliApp([NotNull] this IHostBuilder builder, string[] args, Action<CliOptions> configure = null)
        {
            builder.ConfigureServices((context, collection) =>
                                      {
                                          collection.AddCommandLineArguments(args)
                                                    .AddVersion();

                                          collection.AddCli(configure);

                                          collection.AddCliAppHostedService();

                                          collection.AddCliOptionsBase();
                                      });

            return new CliHostBuilderProxy(builder);
        }

        [NotNull]
        public static IHostBuilder UseConsoleProgramCancellation([NotNull] this IHostBuilder builder, [NotNull] Func<ConsoleKeyInfo, bool> configure)
        {
            builder.ConfigureServices((context, collection) =>
                                      {
                                          collection.AddOptions();
                                          collection.Configure<ConsoleProgramCancellationOptions>(options => options.KeyPredicate = configure);
                                          collection.AddHostedService<ConsoleProgramCancelHostedService>();
                                      });

            return builder;
        }
    }

    public class ConsoleProgramCancellationOptions
    {
        public Func<ConsoleKeyInfo, bool> KeyPredicate { get; set; }
    }

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
            _logger = logger;
            _applicationLifetime = applicationLifetime;
            _options = options?.Value ?? new ConsoleProgramCancellationOptions();
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