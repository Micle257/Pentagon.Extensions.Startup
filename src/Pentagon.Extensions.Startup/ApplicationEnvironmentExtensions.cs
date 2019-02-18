// -----------------------------------------------------------------------
//  <copyright file="ApplicationEnvironmentExtensions.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Startup
{
    public static class ApplicationEnvironmentExtensions
    {
        public static bool IsDevelopment(this IApplicationEnvironment env) => env.EnvironmentName == ApplicationEnvironmentNames.Development;

        public static bool IsProduction(this IApplicationEnvironment env) => env.EnvironmentName == ApplicationEnvironmentNames.Production;

        public static bool IsStaging(this IApplicationEnvironment env) => env.EnvironmentName == ApplicationEnvironmentNames.Staging;
    }
}