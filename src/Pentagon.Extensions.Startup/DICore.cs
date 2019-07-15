namespace Pentagon.Extensions.Startup {
    using System;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public static class DICore
    {
        public static ILogger Logger => Get<ILogger>();

        public static IConfiguration Configuration => Get<IConfiguration>();

        public static IApplicationEnvironment Environment => Get<IApplicationEnvironment>();

        public static AppCore App { get; set; }

        public static TService Get<TService>()
        {
            return (TService) Get(typeof(TService));
        }

        public static object Get(Type serviceType)
        {
            if (App == null)
            {
                throw new ArgumentNullException($"{nameof(DICore)}.{nameof(App)}", $"The App property must be set in order to use {nameof(DICore)} class.");
            }

            if (App.Services == null)
            {
                throw new ArgumentNullException($"{nameof(DICore)}.{nameof(App)}.{nameof(App.Services)}", $"The App is not properly built: cannot resolve services in {nameof(DICore)} class.");
            }

            try
            {
                return App.Services.GetRequiredService(serviceType);
            }
            catch (InvalidOperationException e)
            {
                //Logger?.LogError(e, $"The type {serviceType.Name} cannot be resolved via IoC.");
                throw;
            }
        }
    }
}