// -----------------------------------------------------------------------
//  <copyright file="HostBuilderExtensions.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Startup.Cli
{
    using System;
    using System.Linq;
    using Console.Cli;
    using JetBrains.Annotations;
    using Logging;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Hosting;
    using Serilog;
    using Serilog.Debugging;
    using Serilog.Sinks.SystemConsole.Themes;

    public static class HostBuilderExtensions
    {
        [NotNull]
        public static IHostBuilder UseCliApp([NotNull] this IHostBuilder builder, string[] args, Action<CliStartupOptions> configure = null)
        {
            builder.ConfigureServices((context, collection) =>
                                      {
                                          collection.AddCommandLineArguments(args)
                                                    .AddVersion();

                                          collection.AddCli();

                                          collection.AddCliAppHostedService();

                                          collection.AddCliOptionsBase();

                                          collection.Configure<CliStartupOptions>(context.Configuration.GetSection("CliStartup"));

                                          if (configure != null)
                                              collection.Configure(configure);
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

        /// <summary> Configures the application for serilog logging. </summary>
        /// <param name="hostBuilder"> The host builder. </param>
        /// <param name="runnerName"> Name of the runner. </param>
        /// <returns> The host build for further chaining. </returns>
        [NotNull]
        public static IHostBuilder UseCliSerilogLogging([NotNull] this IHostBuilder hostBuilder, string runnerName = "CLI", Action<LoggerConfiguration> configureSerilog = null)
        {
            return hostBuilder.UseSerilog((context, configuration) =>
                                          {
                                              var loggingOptions = context?.Configuration?.GetSection(key: "Logging").Get<CliLoggingOptions>() ?? new CliLoggingOptions();

                                              if (loggingOptions.InternalLogging)
                                                  SelfLog.Enable(output: Console.Out);

                                              configuration.ReadFrom.Configuration(configuration: context.Configuration, sectionName: "Logging");

                                              var template = "[{Level:u3}] {Message:lj} {NewLine}{Exception}";

                                              if (loggingOptions.Console != null)
                                              {
                                                  switch (loggingOptions.Console.Template)
                                                  {
                                                      case "Normal": break;
                                                      case "Verbose":
                                                          template = "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} <s:{SourceContext}>{NewLine}{Exception}";
                                                          break;
                                                      default:
                                                          template = loggingOptions.Console.Template;
                                                          break;
                                                  }
                                              }

                                              if (!context.Configuration.GetSection("Logging:WriteTo").GetChildren().Any(a => a["Name"] == "Console"))
                                              {
                                                  configuration.WriteTo.Console(outputTemplate: template,
                                                                                theme: AnsiConsoleTheme.Code);
                                              }

                                              if (loggingOptions.HostingEnvironment)
                                                  configuration.Enrich.WithProperty("HostingEnvironment", context.HostingEnvironment.EnvironmentName);

                                              if (loggingOptions.RunId)
                                                  configuration.Enrich.WithProperty("RunId", Guid.NewGuid());

                                              if (!string.IsNullOrWhiteSpace(value: runnerName))
                                                  configuration.Enrich.WithProperty(name: "Runner", value: runnerName);

                                              configureSerilog?.Invoke(configuration);

                                              StaticLoggingOptions.Options = (loggingOptions?.MethodLogging).GetValueOrDefault() ? MethodLogOptions.All : 0;
                                          });
        }
    }
}