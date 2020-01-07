// -----------------------------------------------------------------------
//  <copyright file="ApplicationConfigurationBuilderExtensions.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Startup.Cli
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using Helpers;
    using JetBrains.Annotations;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Options;

    public static class ApplicationConfigurationBuilderExtensions
    {
        [NotNull]
        public static IApplicationBuilder AddCliCommands([NotNull] this IApplicationBuilder builder, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

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

                builder.Services.Add(new ServiceDescriptor(typeof(ICliHandler<>).MakeGenericType(optionType), typeInfo, serviceLifetime));
            }

            return builder;
        }

        [NotNull]
        public static IApplicationBuilder AddCliOptions<TOptions>([NotNull] this IApplicationBuilder builder, [CanBeNull] string name, CliOptionsDelegate<TOptions> configure = null)
                where TOptions : class, new()
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));

            name = name ?? Options.DefaultName;

            var tokenSource = new CliCommandChangeTokenSource<TOptions>(name);

            builder.Services
                   .AddOptions()
                   .AddSingleton<ICliOptionsSource<TOptions>>(tokenSource)
                   .AddSingleton<IOptionsChangeTokenSource<TOptions>>(tokenSource);

            if (configure != null)
            {
                builder.Services.AddSingleton<IPostConfigureOptions<TOptions>>(c => new PostConfigureOptions<TOptions>(name,
                                                                                                               options =>
                                                                                                               {
                                                                                                                   var source = c.GetServices<ICliOptionsSource<TOptions>>()
                                                                                                                                 .FirstOrDefault(a => a.Name == name);

                                                                                                                   var cliOptions = source?.Options;

                                                                                                                   configure(options, cliOptions);
                                                                                                               }));
            }

            return builder;
        }

        [NotNull]
        public static IApplicationBuilder AddCliOptions<TOptions>([NotNull] this IApplicationBuilder builder, CliOptionsDelegate<TOptions> configure = null)
                where TOptions : class, new()
        {
            return AddCliOptions(builder, null, configure);
        }
    }
}