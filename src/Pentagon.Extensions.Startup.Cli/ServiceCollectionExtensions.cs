// -----------------------------------------------------------------------
//  <copyright file="ServiceCollectionExtensions.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Startup.Cli
{
    using System;
    using System.Linq;
    using System.Threading;
    using Helpers;
    using JetBrains.Annotations;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Options;

    public static class ServiceCollectionExtensions
    {
        [NotNull]
        public static IServiceCollection AddCliAppHostedService([NotNull] this IServiceCollection services)
        {
            services.AddSingleton<ICliHostedService, CliHostedService>();

            return services;
        }

        [NotNull]
        public static IServiceCollection AddCliAppHostedService<T>([NotNull] this IServiceCollection services)
                where T : class, ICliHostedService
        {
            services.AddSingleton<ICliHostedService, T>();

            return services;
        }

        [NotNull]
        public static IServiceCollection AddCliOptionsBase([NotNull] this IServiceCollection services)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            services.AddOptions()
                    .TryAddSingleton<ICliOptionsUpdateService, CliOptionsUpdateService>();

            return services;
        }

        [NotNull]
        public static IServiceCollection AddCliOptions<TOptions>([NotNull] this IServiceCollection services, [CanBeNull] string name, CliOptionsDelegate<TOptions> configure = null)
                where TOptions : class, new()
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            name = name ?? Options.DefaultName;

            var tokenSource = new CliCommandChangeTokenSource<TOptions>(name: name);

            services.AddCliOptionsBase()
                   .AddSingleton<ICliOptionsSource<TOptions>>(tokenSource)
                   .AddSingleton<IOptionsChangeTokenSource<TOptions>>(tokenSource);

                services.AddSingleton<IPostConfigureOptions<TOptions>>(c => new PostConfigureOptions<TOptions>(name: name,
                                                                                                                       options =>
                                                                                                                       {
                                                                                                                           var source = c.GetServices<ICliOptionsSource<TOptions>>()
                                                                                                                                         .FirstOrDefault(a => a.Name == name);

                                                                                                                           var cliOptions = source?.Options;

                                                                                                                           if (configure != null)
                                                                                                                               configure(original: options, cli: cliOptions);
                                                                                                                           else
                                                                                                                              TypeHelper.MapAutoProperties(options, cliOptions);
                                                                                                                       }));

            return services;
        }

       

        [NotNull]
        public static IServiceCollection AddCliOptions<TOptions>([NotNull] this IServiceCollection services, CliOptionsDelegate<TOptions> configure = null)
                where TOptions : class, new() =>
                AddCliOptions(services: services, null, configure: configure);
    }

    static class TypeHelper
    {
      public   static void MapAutoProperties<TOptions>(TOptions options, TOptions cliOptions)
                where TOptions : class, new()
        {
            foreach (var autoProperty in typeof(TOptions).GetAutoProperties())
            {
                autoProperty.SetValue(options, autoProperty.GetValue(cliOptions));
            }
        }
    }
}