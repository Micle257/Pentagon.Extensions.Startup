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
    using Microsoft.Extensions.Logging;

    public interface IApplicationConfigurationBuilder
    {
        IServiceCollection Services { get; }
        IApplicationEnvironment Environment { get; }
        IConfiguration Configuration { get; }

        IApplicationConfigurationBuilder UseEnvironment(string environment);
        IApplicationConfigurationBuilder AddConfiguration(Action<IConfigurationBuilder> configure = null);

        IApplicationConfigurationBuilder AddLogging();
        IApplicationConfigurationBuilder AddLogging(Action<ILoggingBuilder> configure);
        IApplicationConfigurationBuilder AddLogging(string defaultLoggerName, Action<ILoggingBuilder> configure);
        IApplicationConfigurationBuilder AddLogging(string defaultLoggerName);

        ApplicationBuilderResult Build();

        IEnumerable<(string Text, LogLevel Level)> GetLoggerLines();
    }
}