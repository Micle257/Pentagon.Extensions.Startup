namespace Pentagon.Extensions.Startup.Cli {
    using System;
    using JetBrains.Annotations;

    public interface ICliOptionsUpdateService
    {
        void UpdateOptions(object options);

        void UpdateOptions(string name, object options);

        void UpdateOptions<TOptions>([CanBeNull] Action<TOptions> updateCallback)
                where TOptions : class, new();

        void UpdateOptions<TOptions>(string name, [CanBeNull] Action<TOptions> updateCallback)
                where TOptions : class, new();

        void UpdateOptions<TOptions>(TOptions options)
                where TOptions : class, new();

        void UpdateOptions<TOptions>(string name, TOptions options)
                where TOptions : class, new();
    }
}