// -----------------------------------------------------------------------
//  <copyright file="ApplicationConfigurationBuilder.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Startup
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Logging;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public class ApplicationConfigurationBuilder : IApplicationConfigurationBuilder
    {
        string _defaultLoggerName;
        bool _isLoggingAdded;
        public IList<(LogLevel Level, LoggerState State, Exception Exception)> BuildLog { get; } = new List<(LogLevel Level, LoggerState State, Exception Exception)>();

        /// <inheritdoc />
        public IServiceCollection Services { get; } = new ServiceCollection();

        /// <inheritdoc />
        public IApplicationEnvironment Environment { get; private set; }

        /// <inheritdoc />
        public IConfiguration Configuration { get; private set; }

        /// <inheritdoc />
        public IApplicationConfigurationBuilder UseEnvironment(string environment)
        {
            Environment = new ApplicationEnvironment(environment);

            return this;
        }
        
        /// <inheritdoc />
        public IApplicationConfigurationBuilder AddConfiguration(Action<IConfigurationBuilder> configure = null)
        {
            var configurationBuilder = new ConfigurationBuilder()
                                       .AddEnvironmentVariables()
                                       .SetBasePath(Directory.GetCurrentDirectory())
                                       .AddJsonFile(path: "appsettings.json", optional: false, reloadOnChange: true);

            if (Environment != null)
                configurationBuilder.AddJsonFile($"appsettings.{Environment.Name}.json", true, true);
            else
                BuildLog.Add((LogLevel.Warning, LoggerState.FromCurrentPosition(message: "Environment is not specified. Configuration of environment specific file(s) is skipped."), null));

            configure?.Invoke(configurationBuilder);

            var configuration = configurationBuilder.Build();

            Configuration = configuration;

            return this;
        }

        /// <inheritdoc />
        public IApplicationConfigurationBuilder AddLogging() => AddLogging(defaultLoggerName: "Unspecified", configure: null);

        /// <inheritdoc />
        public IApplicationConfigurationBuilder AddLogging(Action<ILoggingBuilder> configure) => AddLogging(defaultLoggerName: "Unspecified", configure: configure);

        /// <inheritdoc />
        public IApplicationConfigurationBuilder AddLogging(string defaultLoggerName, Action<ILoggingBuilder> configure)
        {
            Services.AddLogging(options =>
                                {
                                    if (Configuration != null)
                                        options.AddConfiguration(Configuration.GetSection(key: "Logging"));
                                    else
                                        BuildLog.Add((LogLevel.Warning, LoggerState.FromCurrentPosition(message: "Configuration is not specified. Default logging option are used."), null));

                                    options.AddConsole()
                                           .AddDebug();

                                    configure?.Invoke(options);
                                });

            _defaultLoggerName = defaultLoggerName;

            _isLoggingAdded = true;
            return this;
        }

        /// <inheritdoc />
        public IApplicationConfigurationBuilder AddLogging(string defaultLoggerName) => AddLogging(defaultLoggerName, null);
        
        /// <inheritdoc />
        public ApplicationBuilderResult Build()
        {
            if (Environment == null)
            {
                Environment = new ApplicationEnvironment("Unknown");
                BuildLog.Add((LogLevel.Warning, LoggerState.FromCurrentPosition(message: "Environment is not specified."), null));
            }

            Services.AddSingleton(Environment);

            if (Configuration != null)
                Services.AddSingleton(Configuration);
            else
                BuildLog.Add((LogLevel.Warning, LoggerState.FromCurrentPosition(message: "Configuration is not specified and won't be added to services."), null));

            if (_defaultLoggerName != null && _isLoggingAdded)
                Services.AddTransient(provider => provider.GetService<ILoggerFactory>().CreateLogger(_defaultLoggerName));

            var result = new ApplicationBuilderResult(Services.BuildServiceProvider(), BuildLog);

            return result;
        }

        /// <inheritdoc />
        public IEnumerable<(string Text, LogLevel Level)> GetLoggerLines()
        {
            return BuildLog.Select(a => (LoggerSourceFormatter.GetLogMessage(a.State, a.Exception), a.Level));
        }
    }
}