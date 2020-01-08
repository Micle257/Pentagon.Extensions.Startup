// -----------------------------------------------------------------------
//  <copyright file="HostBuilderExtensions.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Startup.Cli
{
    using System;
    using Console.Cli;
    using JetBrains.Annotations;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Hosting;

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
}