// -----------------------------------------------------------------------
//  <copyright file="ApplicationEnvironmentNames.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Startup
{
    public static class ApplicationEnvironmentNames
    {
        public const string Development = "Development";
        public const string Production = "Production";
    }

    public static class ApplicationEnvironmentExtensions
    {
        public static bool IsDevelopment(this IApplicationEnvironment env)
        {
            return env.Name == ApplicationEnvironmentNames.Development;
        }

        public static bool IsProduction(this IApplicationEnvironment env)
        {
            return env.Name == ApplicationEnvironmentNames.Production;
        }
    }
}