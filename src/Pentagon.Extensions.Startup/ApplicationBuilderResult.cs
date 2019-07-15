// -----------------------------------------------------------------------
//  <copyright file="ApplicationBuilderResult.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Startup
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Logging;
    using Microsoft.Extensions.Logging;

    public class ApplicationBuilderResult
    {
        public ApplicationBuilderResult(IServiceProvider provider)
        {
            Provider = provider;
        }

        public IServiceProvider Provider { get; }
    }
}