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
    using Microsoft.Extensions.DependencyInjection;

    public static class ApplicationConfigurationBuilderExtensions
    {
        public static IApplicationBuilder AddCliCommands(this IApplicationBuilder builder, ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
        {
            var commands = AppDomain.CurrentDomain
                                    .GetAssemblies()
                                    .SelectMany(a => a.GetTypes())
                                    .Where(a => a.GetInterfaces().Any(b => b.IsGenericType && b.GetGenericTypeDefinition() == typeof(ICliCommand<>)));

            foreach (var typeInfo in commands)
            {
                var optionType = typeInfo.GetInterfaces()
                                         .FirstOrDefault(a => a.GetGenericTypeDefinition() == typeof(ICliCommand<>))?
                                         .GenericTypeArguments?
                                         .FirstOrDefault();

                builder.Services.Add(new ServiceDescriptor(typeof(ICliCommand<>).MakeGenericType(optionType), typeInfo, serviceLifetime));
            }

            return builder;
        }
    }
}