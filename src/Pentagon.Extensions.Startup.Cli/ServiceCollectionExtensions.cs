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
        public static IServiceCollection AddCliAppAsHostedService([NotNull] this IServiceCollection services)
        {
            services.AddHostedService<CliApp>();
            services.AddSingleton<ICliHostedService>(c => c.GetRequiredService<CliApp>());

            return services;
        }

        [NotNull]
        public static IServiceCollection AddCliAppAsHostedService<T>([NotNull] this IServiceCollection services)
                where T : class, ICliHostedService
        {
            services.AddHostedService<T>();
            services.AddSingleton<ICliHostedService>(c => c.GetRequiredService<T>());

            return services;
        }

        [NotNull]
        public static IServiceCollection AddCliCommands([NotNull] this IServiceCollection services, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            var commands = AppDomain.CurrentDomain
                                    .GetLoadedTypes()
                                    .Where(a => !a.IsAbstract)
                                    .Where(a => a.GetInterfaces().Any(b => b.IsGenericType && b.GetGenericTypeDefinition() == typeof(ICliHandler<>)));

            foreach (var typeInfo in commands)
            {
                var optionType = typeInfo.GetInterfaces()
                                         .FirstOrDefault(a => a.GetGenericTypeDefinition() == typeof(ICliHandler<>))?
                                         .GenericTypeArguments?
                                         .FirstOrDefault();

                services.Add(new ServiceDescriptor(typeof(ICliHandler<>).MakeGenericType(optionType), implementationType: typeInfo, lifetime: serviceLifetime));
            }

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

            services.AddOptions()
                   .AddSingleton<ICliOptionsSource<TOptions>>(tokenSource)
                   .AddSingleton<IOptionsChangeTokenSource<TOptions>>(tokenSource);

            if (configure != null)
            {
                services.AddSingleton<IPostConfigureOptions<TOptions>>(c => new PostConfigureOptions<TOptions>(name: name,
                                                                                                                       options =>
                                                                                                                       {
                                                                                                                           var source = c.GetServices<ICliOptionsSource<TOptions>>()
                                                                                                                                         .FirstOrDefault(a => a.Name == name);

                                                                                                                           var cliOptions = source?.Options;

                                                                                                                           configure(original: options, cli: cliOptions);
                                                                                                                       }));
            }

            return services;
        }

        [NotNull]
        public static IServiceCollection AddCliOptions<TOptions>([NotNull] this IServiceCollection services, CliOptionsDelegate<TOptions> configure = null)
                where TOptions : class, new() =>
                AddCliOptions(services: services, null, configure: configure);
    }
}