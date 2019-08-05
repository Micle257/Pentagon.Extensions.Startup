// -----------------------------------------------------------------------
//  <copyright file="ApplicationConfigurationBuilder.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Startup
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
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
        [NotNull]
        internal static HashSet<string> Defined = new HashSet<string>(ApplicationEnvironmentNames.Defined, StringComparer.InvariantCultureIgnoreCase);

        string _defaultLoggerName = "Default";

        bool _isLoggingAdded;

        public ApplicationBuilder()
        {
            Configuration = new ConfigurationBuilder()
                            .SetBasePath(Directory.GetCurrentDirectory())
                            .Build();
        }

        public ILogger BuildLog { get; set; } 

        /// <inheritdoc />
        public IServiceCollection Services { get; } = new ServiceCollection();

        /// <inheritdoc />
        public IApplicationEnvironment Environment { get; private set; }

        /// <inheritdoc />
        public IApplicationVersion Version { get; private set; }

        /// <inheritdoc />
        public IConfiguration Configuration { get; private set; }

        /// <inheritdoc />
        public IApplicationBuilder AttachInnerLogger(ILogger logger)
        {
            BuildLog = logger;

            return this;
        }

        /// <inheritdoc />
        public IApplicationBuilder AddEnvironmentVariables()
        {
            AddConfiguration(builder => builder.AddEnvironmentVariables());

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

        /// <inheritdoc />
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
        public IApplicationBuilder AddVersion(Assembly assembly = null)
        {
            try
            {
                var version = ApplicationVersion.Create(assembly);

                Version = version;
            }
            catch (Exception e)
            {
                BuildLog?.LogError(e, "Adding version failed.");
            }

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
                                         BuildLog?.LogWarning(new ArgumentNullException(nameof(Environment), message: "The environment of app is null; environment specific configuration file cannot be used."), "The environment of app is null; environment specific configuration file cannot be used.");
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
                                        BuildLog.LogWarning("Configuration is not specified. Default logging option will be used.");

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

            if (Version == null)
                AddVersion(null);

            Services.AddSingleton(Environment);
            Services.AddSingleton(Version);
            Services.AddSingleton(Configuration);

            if (_isLoggingAdded)
                Services.AddTransient(provider => provider.GetService<ILoggerFactory>().CreateLogger(Environment.ApplicationName ?? _defaultLoggerName));

            var result = new ApplicationBuilderResult(Services.BuildServiceProvider());

            return result;
        }
    }
}