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
    using System.Threading;
    using JetBrains.Annotations;
    using Logging;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.FileProviders;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Extensions.Primitives;

    public class ApplicationBuilder : IApplicationBuilder
    {
        string _defaultLoggerName = "Default";
        bool _isLoggingAdded;

        public ApplicationBuilder()
        {
            Configuration = new ConfigurationBuilder()
                            .AddEnvironmentVariables()
                            .SetBasePath(Directory.GetCurrentDirectory())
                            .Build();
        }

        public IList<(LogLevel Level, LoggerState State, Exception Exception)> BuildLog { get; } = new List<(LogLevel Level, LoggerState State, Exception Exception)>();

        /// <inheritdoc />
        public IServiceCollection Services { get; } = new ServiceCollection();

        /// <inheritdoc />
        public IApplicationEnvironment Environment { get; private set; }

        /// <inheritdoc />
        public IConfiguration Configuration { get; private set; }

        /// <inheritdoc />
        public IApplicationBuilder DefineEnvironment(string environment)
        {
            ApplicationEnvironmentNames.Define(environment);

            return this;
        }

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

        public IApplicationBuilder AddEnvironmentFromEnvironmentVariable(string variableName = null)
        {
            var env = variableName != null
                              ? System.Environment.GetEnvironmentVariable(variableName)
                              : System.Environment.GetEnvironmentVariable(variable: "ASPNET_ENVIRONMENT")
                                ?? System.Environment.GetEnvironmentVariable(variable: "ASPNETCORE_ENVIRONMENT")
                                ?? Assembly.GetEntryAssembly()?.GetCustomAttribute<DefaultEnvironmentAttribute>()?.DefaultEnvironmentName;

            if (env == null || !ApplicationEnvironmentExtensions.IsValid(env))
                env = ApplicationEnvironmentNames.Production;

            var ass = Assembly.GetEntryAssembly().GetName().Name;

            Environment = new ApplicationEnvironment
                          {
                                  EnvironmentName = env,
                                  ApplicationName = ass,
                                  ContentRootPath = Directory.GetCurrentDirectory()
                          };

            return this;
        }

        /// <inheritdoc />
        public IApplicationBuilder AddJsonFileConfiguration(bool useEnvironmentSpecific = false,
                                                            string name = "appsettings",
                                                            IFileProvider fileProvider = null)
        {
            AddConfiguration(builder =>
                             {
                                 if (fileProvider == null)
                                     builder.AddJsonFile($"{name}.json", true, true);
                                 else
                                     builder.AddJsonFile(fileProvider, $"{name}.json", true, true);

                                 if (useEnvironmentSpecific)
                                 {
                                     if (Environment == null)
                                     {
                                         BuildLog.Add((LogLevel.Warning, LoggerState.FromCurrentPosition(message: "The environment of app is null; environment specific configuration file cannot be used."),
                                                          new ArgumentNullException(nameof(Environment), message: "The environment of app is null; environment specific configuration file cannot be used.")));
                                     }
                                     else
                                     {
                                         if (fileProvider == null)
                                             builder.AddJsonFile($"{name}.{Environment.EnvironmentName}.json", true, true);
                                         else
                                             builder.AddJsonFile(fileProvider, $"{name}.{Environment.EnvironmentName}.json", true, true);
                                     }
                                 }
                             });

            return this;
        }

        /// <inheritdoc />
        public IApplicationBuilder AddCommandLineArguments([CanBeNull] string[] args, string configPrefix = "CommandLineArguments")
        {
            if (args == null || args.Length == 0)
                return this;

            AddConfiguration(builder =>
                             {
                                 var coll = new Dictionary<string, string>();

                                 for (var i = 0; i < args.Length; i++)
                                     coll.Add($"{configPrefix}:{i}", args[i]);

                                 builder.AddInMemoryCollection(coll);
                             });

            return this;
        }

        /// <inheritdoc />
        public IApplicationBuilder AddConfiguration([CanBeNull] Action<IConfigurationBuilder> configure)
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
            => AddLogging(Configuration.GetSection(key: "Logging"), configure);

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
                AddEnvironmentFromEnvironmentVariable();

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
            return BuildLog.Select(a => (LoggerSourceFormatter.GetLogMessage(a.State, null, a.Exception), a.Level));
        }
    }
}