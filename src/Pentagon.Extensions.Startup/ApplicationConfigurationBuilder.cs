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
    using System.Reflection;
    using Logging;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.FileProviders;
    using Microsoft.Extensions.Logging;

    public class ApplicationBuilder : IApplicationBuilder
    {
        string _defaultLoggerName = "Unspecified";
        bool _isLoggingAdded;
        public IList<(LogLevel Level, LoggerState State, Exception Exception)> BuildLog { get; } = new List<(LogLevel Level, LoggerState State, Exception Exception)>();

        public ApplicationBuilder()
        {
            Configuration = new ConfigurationBuilder()
                            .AddEnvironmentVariables()
                                                      .SetBasePath(Directory.GetCurrentDirectory())
                    .Build();
        }

        /// <inheritdoc />
        public IServiceCollection Services { get; } = new ServiceCollection();

        /// <inheritdoc />
        public IApplicationEnvironment Environment { get; private set; }

        /// <inheritdoc />
        public IConfiguration Configuration { get; private set; }

        /// <inheritdoc />
        public IApplicationBuilder AddEnvironment(string environment)
        {
            var ass = Assembly.GetEntryAssembly().GetName().Name;

            Environment = new ApplicationEnvironment
            {
                EnvironmentName = environment,
                ApplicationName = ass,
                ContentRootPath = Directory.GetCurrentDirectory()
            };

            return this;
        }

        public IApplicationBuilder AddEnvironmentFromEnvironmentVariable(string variableName = "ASPNETCORE_ENVIRONMENT")
        {
            var ass = Assembly.GetEntryAssembly().GetName().Name;

            var env = System.Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            if (env == null || !ApplicationEnvironmentExtensions.IsValidName(env))
                env = ApplicationEnvironmentNames.Production;

            Environment = new ApplicationEnvironment
                          {
                                  EnvironmentName = env,
                                  ApplicationName = ass,
                                  ContentRootPath = Directory.GetCurrentDirectory()
                          };

            return this;
        }

        /// <inheritdoc />
        public IApplicationBuilder AddJsonFileConfiguration(bool useEnvironmentSpecific = true,
                                                                     string name = "appsettings",
                                                                     IFileProvider fileProvider = null)
        {
            AddConfiguration(builder =>
                             {
                                 if (fileProvider == null)
                                     builder.AddJsonFile($"{name}.json", true, true);
                                 else builder.AddJsonFile(fileProvider, $"{name}.json", true, true);

                                 if (useEnvironmentSpecific)
                                 {
                                     if (fileProvider == null)
                                         builder.AddJsonFile($"{name}.{Environment.EnvironmentName}.json", true, true);
                                     else builder.AddJsonFile(fileProvider, $"{name}.{Environment.EnvironmentName}.json", true, true);
                                 }
                             });

            return this;
        }
        
        /// <inheritdoc />
        public IApplicationBuilder AddCommandLineArguments(string[] args)
        {
            if (args == null || args.Length == 0)
                return this;

            AddConfiguration(builder =>
                             {
                                 var coll = new Dictionary<string, string>();

                                 for (var i = 0; i < args.Length; i++)
                                 {
                                     coll.Add($"CommandLineArguments:{i}", args[i]);
                                 }

                                 builder.AddInMemoryCollection(coll);
                             });

            return this;
        }
        
        /// <inheritdoc />
        public IApplicationBuilder AddConfiguration(Action<IConfigurationBuilder> configure)
        {
            if (configure == null)
                return this;

            // create builder
            var configurationBuilder = new ConfigurationBuilder()
                                       // adds current config
                                       .AddConfiguration(Configuration);

            // uses the new config
            configure.Invoke(configurationBuilder);

            var configuration = configurationBuilder.Build();

            Configuration = configuration;

            return this;
        }

        /// <inheritdoc />
        public IApplicationBuilder AddLogging() => AddLogging(configure: null);

        /// <inheritdoc />
        public IApplicationBuilder AddLogging(Action<ILoggingBuilder> configure)
            => AddLogging(Configuration.GetSection("Logging"), configure);

        /// <inheritdoc />
        public IApplicationBuilder AddLogging(IConfiguration configuration)
            => AddLogging(configuration, null);

        /// <inheritdoc />
        public IApplicationBuilder AddLogging(IConfiguration configuration, Action<ILoggingBuilder> configure)
        {
            Services.AddLogging(options =>
                                {
                                    if (configuration != null)
                                        options.AddConfiguration(configuration);
                                    else
                                        BuildLog.Add((LogLevel.Warning, LoggerState.FromCurrentPosition(message: "Configuration is not specified. Default logging option will be used."), null));

                                    configure?.Invoke(options);
                                });

            _isLoggingAdded = true;
            return this;
        }

        /// <inheritdoc />
        public IApplicationBuilder AddDefaultLogger(string name)
        {
            _defaultLoggerName = name;

            return this;
        }

        /// <inheritdoc />
        public ApplicationBuilderResult Build()
        {
            if (Environment == null)
            {
                AddEnvironmentFromEnvironmentVariable();
            }

            Services.AddSingleton(Environment);
            Services.AddSingleton(Configuration);

            if (_isLoggingAdded)
                Services.AddTransient(provider => provider.GetService<ILoggerFactory>().CreateLogger(Environment.ApplicationName ?? _defaultLoggerName));

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