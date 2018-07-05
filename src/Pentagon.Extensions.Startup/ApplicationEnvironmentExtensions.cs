namespace Pentagon.Extensions.Startup {
    public static class ApplicationEnvironmentExtensions
    {
        public static bool IsDevelopment(this IApplicationEnvironment env)
        {
            return env.EnvironmentName == ApplicationEnvironmentNames.Development;
        }

        public static bool IsProduction(this IApplicationEnvironment env)
        {
            return env.EnvironmentName == ApplicationEnvironmentNames.Production;
        }

        public static bool IsStaging(this IApplicationEnvironment env)
        {
            return env.EnvironmentName == ApplicationEnvironmentNames.Staging;
        }
    }
}