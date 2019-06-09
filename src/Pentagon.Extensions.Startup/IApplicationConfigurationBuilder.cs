﻿// -----------------------------------------------------------------------
//  <copyright file="IApplicationConfigurationBuilder.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Startup
{
    using System;
    using System.Collections.Generic;
    using JetBrains.Annotations;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.FileProviders;
    using Microsoft.Extensions.Logging;

    public interface IApplicationBuilder
    {
        IServiceCollection Services { get; }

        IApplicationEnvironment Environment { get; }

        IConfiguration Configuration { get; }

        /// <summary>
        /// Defines a new environment name.
        /// </summary>
        /// <param name="environment">The environment.</param>
        /// <returns>The <see cref="IApplicationBuilder"/> instance that called this method.</returns>
        IApplicationBuilder DefineEnvironment(string environment);

        IApplicationBuilder AddEnvironment(string environment);

        IApplicationBuilder AddEnvironmentFromEnvironmentVariable(string variableName = "ASPNETCORE_ENVIRONMENT");

        IApplicationBuilder AddCommandLineArguments([CanBeNull] string[] args, string configPrefix = "CommandLineArguments");

        IApplicationBuilder AddJsonFileConfiguration(bool useEnvironmentSpecific = true,
                                                                  string name = "appsettings",
                                                                  IFileProvider fileProvider = null);

        IApplicationBuilder AddConfiguration([CanBeNull] Action<IConfigurationBuilder> configure = null);

        IApplicationBuilder AddLogging();

        IApplicationBuilder AddLogging(Action<ILoggingBuilder> configure);

        IApplicationBuilder AddLogging(IConfiguration configuration);

        IApplicationBuilder AddLogging(IConfiguration configuration, Action<ILoggingBuilder> configure);

        IApplicationBuilder AddDefaultLogger(string name);

        ApplicationBuilderResult Build();

        IEnumerable<(string Text, LogLevel Level)> GetLoggerLines();
    }
}