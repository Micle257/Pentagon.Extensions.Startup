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

            if (execution == AppExecutionType.LoopRun)
            {
                var currentIteration = 0;

                do
                {
                    using (var scope = Services.CreateScope())
                    {
                        var context = AppExecutionContext.Create(scope);

                        context.IterationCount = currentIteration;

                        try
                        {
                            await ExecuteScopedCoreAsync(context);
                        }
                        catch
                        {
                            throw;
                        }

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
            }

            if (execution == AppExecutionType.SingleRun)
            {
                var result = await ExecuteCoreAsync(Services);

                return result;
            }

            return 0;
        }

        protected virtual Task<int> ExecuteCoreAsync(IServiceProvider provider) => Task.FromResult(0);

        protected virtual Task ExecuteScopedCoreAsync(AppExecutionContext context)
        {
            context.TerminationRequested = true;
            context.Result = 0;

            return Task.FromResult(context);
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