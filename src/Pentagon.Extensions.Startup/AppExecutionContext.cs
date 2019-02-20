// -----------------------------------------------------------------------
//  <copyright file="AppExecutionContext.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Startup
{
    using System;
    using Microsoft.Extensions.DependencyInjection;

    public class AppExecutionContext
    {
        public IServiceScope Scope { get; private set; }

        public IServiceProvider Provider { get; private set; }

        public int Result { get; set; }

        public bool TerminationRequested { get; set; }

        public int IterationCount { get; internal set; }
        
        internal static AppExecutionContext Create(IServiceScope scope) =>
                new AppExecutionContext
                {
                        Scope = scope,
                        Provider = scope.ServiceProvider
                };
    }
}