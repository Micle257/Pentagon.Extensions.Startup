namespace Pentagon.Extensions.Startup {
    using System;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    public static class DICore
    {
        public static ILogger Logger => (ILogger)App.Services.GetService(typeof(ILogger));

        public static IConfiguration Configuration => App.Configuration;

        public static AppCore App { get; set; }

        public static TService Get<TService>()
        {
            try
            {
                return (TService)App.Services.GetService(typeof(TService));
            }
            catch (Exception e)
            {
                Logger?.LogCritical(e, $"The type {typeof(TService).Name} cannot be resolved via IoC.");
                throw;
            }
        }
    }
}