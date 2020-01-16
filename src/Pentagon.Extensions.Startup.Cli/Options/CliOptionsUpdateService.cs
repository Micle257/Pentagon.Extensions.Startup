// -----------------------------------------------------------------------
//  <copyright file="CliOptionsUpdateService.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Startup.Cli {
    using System;
    using System.Linq;
    using JetBrains.Annotations;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;

    public class CliOptionsUpdateService : ICliOptionsUpdateService
    {
        IServiceScope _scope;

        public CliOptionsUpdateService([NotNull] IServiceScopeFactory serviceScopeFactory)
        {
            _scope = serviceScopeFactory.CreateScope();
        }

        /// <inheritdoc />
        public void UpdateOptions( object options)
        {
            UpdateOptions(Options.DefaultName, options);
        }

        /// <inheritdoc />
        public void UpdateOptions(string name, object optionsValue)
        {
            var serviceType = typeof(ICliOptionsSource<>).MakeGenericType(optionsValue.GetType());

            var options = _scope.ServiceProvider.GetServices(serviceType)
                                .FirstOrDefault(a => (string)a.GetType().GetProperty(nameof(ICliOptionsSource<int>.Name)).GetValue(a) == name);

            var propertyInfo = options?.GetType().GetProperty(nameof(ICliOptionsSource<int>.Options));

            propertyInfo?.SetValue(options, optionsValue);

            // options.Reload()
            options?.GetType().GetMethod(nameof(ICliOptionsSource<int>.Reload), Array.Empty<Type>())?.Invoke(options, Array.Empty<object>());
        }

        public void UpdateOptions<TOptions>([CanBeNull] Action<TOptions> updateCallback)
                where TOptions : class, new()
        {
            UpdateOptions(Options.DefaultName, updateCallback);
        }

        public void UpdateOptions<TOptions>(string name, [CanBeNull] Action<TOptions> updateCallback)
                where TOptions : class, new()
        {
            var options = _scope.ServiceProvider.GetServices<ICliOptionsSource<TOptions>>()
                                .FirstOrDefault(a => a.Name == name);

            if (options == null)
                return;

            updateCallback?.Invoke(options.Options);

            options.Reload();
        }

        /// <inheritdoc />
        public void UpdateOptions<TOptions>(TOptions options)
                where TOptions : class, new()
        {
            UpdateOptions(Options.DefaultName, options);
        }

        /// <inheritdoc />
        public void UpdateOptions<TOptions>(string name, TOptions optionsValue)
                where TOptions : class, new()
        {
            var options = _scope.ServiceProvider.GetServices<ICliOptionsSource<TOptions>>()
                                .FirstOrDefault(a => a.Name == name);

            if (options == null)
                return;

            TypeHelper.MapAutoProperties(options.Options, optionsValue);

            options.Reload();
        }
    }
}