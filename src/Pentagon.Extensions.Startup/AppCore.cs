// -----------------------------------------------------------------------
//  <copyright file="AppCore.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Startup
{
    using System;
    using System.Threading.Tasks;
    using JetBrains.Annotations;
    using Logging;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    /// <summary> Represents a helper base class for app configuration. Similar to ASP.NET Core 'Startup' class. </summary>
    public abstract class AppCore
    {
        [NotNull]
        readonly object _lock = new object();

        IApplicationBuilder _builder;

        /// <summary> The value indicates if the <see cref="ConfigureServices" /> methods was called. </summary>
        bool _startupCalled;

        /// <summary> Gets the configuration instance. </summary>
        /// <value> The <see cref="IConfiguration" />. </value>
        public IConfiguration Configuration => _builder?.Configuration;

        public IApplicationEnvironment Environment => _builder?.Environment;

        public IServiceProvider Services { get; private set; }

        /// <summary> Starts the startup procedure, configures the DI container. </summary>
        /// <param name="args"> The program arguments. </param>
        public void ConfigureServices(string[] args = null)
        {
            lock (_lock)
            {
                if (_startupCalled)
                    return;

                try
                {
                    _builder = new ApplicationBuilder();

                    _builder.AddLogging()?
                            .AddCommandLineArguments(args);

                    BuildApp(_builder, args);

                    if (_builder == null)
                        throw new ArgumentNullException(nameof(_builder));

                    var result = _builder?.Build();

                    if (result == null)
                        throw new ArgumentNullException(nameof(result));

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

        public int Execute(AppExecutionType execution, ExecutionOptions options = null) => ExecuteAsync(execution, options).Result;

        public async Task<int> ExecuteAsync(AppExecutionType execution, ExecutionOptions options = null)
        {
            if (!_startupCalled)
                throw new InvalidOperationException("Startup hasn't been called yet.");

            if (execution == AppExecutionType.Unspecified)
                throw new ArgumentException(message: "App execution was not specified.");

            if (options == null)
                options = new ExecutionOptions();

            var currentIteration = 0;

            do
            {
                using (var scope = Services.CreateScope())
                {
                    var context = AppExecutionContext.Create(execution == AppExecutionType.LoopRun ? scope.ServiceProvider : Services,
                                                             execution == AppExecutionType.LoopRun ? scope : null,
                                                             execution);

                    context.IterationCount = currentIteration;

                    try
                    {
                        await ExecuteCoreAsync(context).ConfigureAwait(false);
                    }
                    catch
                    {
                        throw;
                    }

                    if (execution == AppExecutionType.SingleRun)
                        return context.Result;

                    if (options.TerminateValue.HasValue && options.TerminateValue.Value == context.Result)
                    {
                        context.TerminationRequested = true;
                    }

                    if (context.TerminationRequested)
                        return context.Result;

                    if (options.LoopWaitMilliseconds > 0)
                        await Task.Delay(options.LoopWaitMilliseconds);

                    currentIteration++;
                }
            } while (true);

            return 0;
        }

        protected virtual Task ExecuteCoreAsync(AppExecutionContext context) => Task.CompletedTask;

        /// <summary> Builds the application. </summary>
        /// <param name="appBuilder"> The application builder. </param>
        /// <param name="args"> The program arguments. </param>
        protected abstract void BuildApp(IApplicationBuilder appBuilder, string[] args);

        protected virtual void OnAppDomainUnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            Services.GetService<ILogger>()?.LogSource(LogLevel.Error,
                                                      message: "Exception unhandled (AppDomain).",
                                                      new EventId(),
                                                      args.ExceptionObject as Exception);
        }

        protected virtual void OnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs args)
        {
            Services.GetService<ILogger>()?.LogSource(LogLevel.Error, message: "Exception unhandled (TaskScheduler).", new EventId(), args.Exception);
        }

        void ConfigureUnhandledExceptionHandling()
        {
            AppDomain.CurrentDomain.UnhandledException += OnAppDomainUnhandledException;
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
        }

        [NotNull]
        public static T New<T>([NotNull] T app, string[] args = null)
            where T : AppCore
        {
            DICore.App = app ?? throw new ArgumentNullException(nameof(app));

            app.ConfigureServices(args);

            return app;
        }
    }
}