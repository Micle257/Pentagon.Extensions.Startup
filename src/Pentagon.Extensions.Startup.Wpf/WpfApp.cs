// -----------------------------------------------------------------------
//  <copyright file="WpfApp.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Startup.Wpf
{
    using System.Windows;
    using System.Windows.Threading;
    using JetBrains.Annotations;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public abstract class WpfApp : AppCore
    {
        public void RegisterUnhandledExceptionHandling([NotNull] Application wpfApplication)
        {
            wpfApplication.DispatcherUnhandledException += OnDispatcherUnhandledException;
        }

        protected virtual void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs args)
        {
            Services.GetService<ILogger>()?.LogError(exception: args.Exception, message: "Exception unhandled (Dispatcher).");
        }
    }
}