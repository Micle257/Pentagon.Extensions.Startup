// -----------------------------------------------------------------------
//  <copyright file="AppCore.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Startup
{
    using System;
    using System.Threading.Tasks;
    using Logging;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    /// <summary> Represents a helper base class for app configuration. Similar to ASP.NET Core 'Startup' class. </summary>
    public abstract class AppCore
    {
        readonly object _lock = new object();

        IApplicationBuilder _builder;

        /// <summary> The value indicates if the <see cref="OnStartup" /> methods was called. </summary>
        bool _startupCalled;

        /// <summary> Gets the configuration instance. </summary>
        /// <value> The <see cref="IConfiguration" />. </value>
        public IConfiguration Configuration => _builder?.Configuration;

        protected IApplicationEnvironment Environment => _builder?.Environment;

        protected IServiceProvider Services { get; private set; }

        /// <summary> Starts the startup procedure, configures the DI container. </summary>
        /// <param name="args"> The program arguments. </param>
        public void OnStartup(string[] args)
        {
            lock (_lock)
            {
                if (_startupCalled)
                    return;

                try
                {
                    _builder = new ApplicationBuilder();

                    _builder.AddLogging()
                            .AddCommandLineArguments(args);

                    BuildApp(_builder, args);

                    var result = _builder.Build();

                    Services = result.Provider;

                    result.ApplyLogMessages(Services.GetService<ILogger>());

                    ConfigureUnhandledExceptionHandling();
                }
                finally
                {
                    _startupCalled = true;
                }
            }
        }

        /// <summary> Builds the application. </summary>
        /// <param name="appBuilder"> The application builder. </param>
        /// <param name="args"> The program arguments. </param>
        protected abstract void BuildApp(IApplicationBuilder appBuilder, string[] args);

        void ConfigureUnhandledExceptionHandling()
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
                                                          {
                                                              Services.GetService<ILogger>()?.LogSource(LogLevel.Error,
                                                                                                        message: "Exception unhandled (AppDomain).",
                                                                                                        new EventId(),
                                                                                                        args.ExceptionObject as Exception);
                                                          };

            TaskScheduler.UnobservedTaskException += (sender, args) =>
                                                     {
                                                         Services.GetService<ILogger>()?.LogSource(LogLevel.Error, message: "Exception unhandled (TaskScheduler).", new EventId(), args.Exception);
                                                     };
        }
    }
}