// -----------------------------------------------------------------------
//  <copyright file="IApplicationConfigurationBuilder.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Startup
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.FileProviders;
    using Microsoft.Extensions.Logging;

    public interface IApplicationBuilder
    {
        IServiceCollection Services { get; }
        IApplicationEnvironment Environment { get; }
        IConfiguration Configuration { get; }

        IApplicationBuilder AddEnvironment(string environment);

        IApplicationBuilder AddCommandLineArguments(string[] args);
        IApplicationBuilder AddJsonFileConfiguration(bool useEnvironmentSpecific = true,
                                                                  string name = "appsettings",
                                                                  IFileProvider fileProvider = null);
        IApplicationBuilder AddConfiguration(Action<IConfigurationBuilder> configure = null);

        IApplicationBuilder AddLogging();
        IApplicationBuilder AddLogging(Action<ILoggingBuilder> configure);
        IApplicationBuilder AddLogging(IConfiguration configuration);
        IApplicationBuilder AddLogging(IConfiguration configuration, Action<ILoggingBuilder> configure);
        IApplicationBuilder AddDefaultLogger(string name);

        ApplicationBuilderResult Build();

        IEnumerable<(string Text, LogLevel Level)> GetLoggerLines();
    }
}