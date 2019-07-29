// -----------------------------------------------------------------------
//  <copyright file="StartupServiceCollectionExtensions.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Startup
{
    using System.Reflection;
    using Microsoft.Extensions.DependencyInjection;

    public static class StartupServiceCollectionExtensions
    {
        public static IServiceCollection AddVersion(this IServiceCollection services, Assembly assembly = null)
        {
            var version = ApplicationVersion.Create(assembly);

            services.AddSingleton<IApplicationVersion>(version);

            return services;
        }
    }
}