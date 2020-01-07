// -----------------------------------------------------------------------
//  <copyright file="IApplicationConfigurationBuilder.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Startup
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using JetBrains.Annotations;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.FileProviders;
    using Microsoft.Extensions.Logging;

    // TODO to IStartupServiceCollection
    public interface IApplicationBuilder
    {
        [NotNull]
        IServiceCollection Services { get; }

        IApplicationEnvironment Environment { get; }

        IApplicationVersion Version { get; }

        [NotNull]
        IConfiguration Configuration { get; }

        [NotNull]
        IApplicationBuilder AttachInnerLogger(ILogger logger);

        [NotNull]
        IApplicationBuilder AddEnvironment(string environment);

        [NotNull]
        IApplicationBuilder AddEnvironmentFromEnvironmentVariable(string variableName = "ASPNETCORE_ENVIRONMENT");

        /// <summary>
        /// Adds the version from the give assembly.
        /// </summary>
        /// <param name="assembly">The assembly. Default is <see cref="Assembly.GetEntryAssembly"/></param>
        /// <returns>The <see cref="IApplicationBuilder"/> instance that called this method.</returns>
        [NotNull]
        IApplicationBuilder AddVersion(Assembly assembly = null);

        [NotNull]
        IApplicationBuilder AddEnvironmentVariables();

        [NotNull]
        IApplicationBuilder AddCommandLineArguments([CanBeNull] string[] args, string configPrefix = "CommandLineArguments");

        [NotNull]
        IApplicationBuilder AddJsonFileConfiguration(bool useEnvironmentSpecific = true,
                                                                  string name = "appsettings",
                                                                  IFileProvider fileProvider = null);

        [NotNull]
        IApplicationBuilder AddConfiguration([CanBeNull] Action<IConfigurationBuilder> configure = null);

        [NotNull]
        IApplicationBuilder AddLogging();

        [NotNull]
        IApplicationBuilder AddLogging(Action<ILoggingBuilder> configure);

        [NotNull]
        IApplicationBuilder AddLogging(IConfiguration configuration);

        [NotNull]
        IApplicationBuilder AddLogging(IConfiguration configuration, Action<ILoggingBuilder> configure);

        [NotNull]
        IApplicationBuilder AddDefaultLogger(string name);

        [NotNull]
        IApplicationBuilder AddOptions<TOptions>([CanBeNull] string sectionName)
                where TOptions : class;

        [NotNull]
        IApplicationBuilder ConfigureServices([NotNull] Action<IServiceCollection> callback);

        [NotNull]
        ApplicationBuilderResult Build();
    }
}