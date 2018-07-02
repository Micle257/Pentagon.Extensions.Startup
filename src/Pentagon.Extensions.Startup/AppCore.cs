namespace Pentagon.Extensions.Startup {
    using System;
    using System.Threading.Tasks;
    using Logging;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public abstract class AppCore
    {
        public IServiceProvider Services { get; private set; }

        public IApplicationEnvironment Environment { get; private set; }

        public IConfiguration Configuration { get; private set; }

        protected bool StartupCalled { get; private set; }

        public void OnStartup(string[] args)
        {
            if (StartupCalled)
                return;

            StartupCalled = true;

            var builder = new ApplicationConfigurationBuilder();
            Configuration = builder.Configuration;

            BuildApp(builder, args);
            
            Environment = builder.Environment;
            
            var result = builder.Build();

            Services = result.Provider;
            
            result.ApplyLogMessages(Services.GetService<ILogger>());

            ConfigureUnhandledExceptionHandling();
        }
        
        protected abstract void BuildApp(IApplicationConfigurationBuilder appBuilder, string[] args);

        void ConfigureUnhandledExceptionHandling()
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
                                                          {
                                                              Services.GetService<ILogger>()?.LogSource(LogLevel.Error,
                                                                                                        message: "Exception unhandled (AppDomain).",
                                                                                                        eventId: new EventId(),
                                                                                                        exception: args.ExceptionObject as Exception);
                                                          };

            TaskScheduler.UnobservedTaskException += (sender, args) =>
                                                     {
                                                         Services.GetService<ILogger>()?.LogSource(LogLevel.Error, message: "Exception unhandled (TaskScheduler).", eventId: new EventId(), exception: args.Exception);
                                                     };
        }
    }
}