// -----------------------------------------------------------------------
//  <copyright file="HostBuilderExtensions.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Startup.Cli
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using JetBrains.Annotations;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;

    public interface ICliHostBuilder
    {
        [NotNull]
        ICliHost Build();

        void RegisterParallelCallback([NotNull] Func<CancellationToken, Task<int>> callback);
    }

    public interface ICliHost
    {

    }

    public static class HostBuilderExtensions
    {
        [NotNull]
        public static IHostBuilder UseCliApp([NotNull] this IHostBuilder builder, string[] args, Action<ICliHostBuilder> configureCallback = null)
        {
            builder.ConfigureServices((context, collection) =>
                                      {
                                          collection.AddCommandLineArguments(args);
                                          collection.AddCliCommands();
                                          collection.AddVersion();

                                          collection.AddCliAppAsHostedService();
                                      });

            return builder;
        }

        [NotNull]
        public static IHostBuilder UseCliApp<T>([NotNull] this IHostBuilder builder, string[] args)
                where T : class, ICliHostedService
        {
            builder.ConfigureServices((context, collection) =>
                                      {
                                          collection.AddCommandLineArguments(args);
                                          collection.AddCliCommands();
                                          collection.AddVersion();

                                          collection.AddCliAppAsHostedService<T>();
                                      });

            return builder;
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
        [NotNull]
        readonly IHostApplicationLifetime _applicationLifetime;

        [NotNull]
        ConsoleProgramCancellationOptions _options;

        public ConsoleProgramCancelHostedService([NotNull] IHostApplicationLifetime applicationLifetime,
                                                 IOptions<ConsoleProgramCancellationOptions> options)
        {
            _applicationLifetime = applicationLifetime;
            _options = options?.Value ?? new ConsoleProgramCancellationOptions();
        }

        /// <inheritdoc />
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (_options.KeyPredicate == null)
            {
                DICore.Logger?.LogDebug("Cancel key handler is null.");
                return;
            }

            if (!Environment.UserInteractive)
            {
                DICore.Logger?.LogDebug("Cancel key handler cannot execute: console handle is not present.");
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
                    DICore.Logger?.LogInformation("Cancel key handler: cancel requested.");
                }

                await Task.Delay(100, stoppingToken).ConfigureAwait(false);
            } while (true);
        }
    }
}