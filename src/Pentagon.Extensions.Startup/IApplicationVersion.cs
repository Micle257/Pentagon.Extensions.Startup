// -----------------------------------------------------------------------
//  <copyright file="IApplicationVersion.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Startup
{
    using System;

    public interface IApplicationVersion
    {
        string ProductVersion { get; }

        Version AssemblyVersion { get; }
    }
}