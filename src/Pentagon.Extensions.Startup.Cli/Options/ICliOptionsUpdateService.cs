namespace Pentagon.Extensions.Startup.Cli {
    using System;
    using JetBrains.Annotations;

    public interface ICliOptionsUpdateService {
        void UpdateOptions<TOptions>([CanBeNull] Action<TOptions> updateCallback);
        void UpdateOptions<TOptions>(string name, [CanBeNull] Action<TOptions> updateCallback);
    }
}