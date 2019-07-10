// -----------------------------------------------------------------------
//  <copyright file="ApplicationVersion.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Startup
{
    using System;

    public class ApplicationVersion : IApplicationVersion
    {
        /// <inheritdoc />
        public string ProductVersion { get; set; }

        /// <inheritdoc />
        public Version AssemblyVersion { get; set; }
    }
}