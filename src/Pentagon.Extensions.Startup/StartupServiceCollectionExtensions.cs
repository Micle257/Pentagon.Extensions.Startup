// -----------------------------------------------------------------------
//  <copyright file="StartupServiceCollectionExtensions.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Startup
{
    using System.Reflection;
    using JetBrains.Annotations;
    using Microsoft.Extensions.DependencyInjection;

    public static class StartupServiceCollectionExtensions
    {
        [NotNull]
        public static IServiceCollection AddVersion([NotNull] this IServiceCollection services, Assembly assembly = null)
        {
            var version = ApplicationVersion.Create(assembly);

            services.AddSingleton<IApplicationVersion>(version);

            return services;
        }

        [NotNull]
        public static IServiceCollection AddCommandLineArguments([NotNull] this IServiceCollection services, string[] args)
        {
            services.AddSingleton<IApplicationArguments>(c => new ApplicationArguments(args));

            return services;
        }
    }
}