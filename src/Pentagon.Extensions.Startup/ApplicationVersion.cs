// -----------------------------------------------------------------------
//  <copyright file="ApplicationVersion.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Startup
{
    using System;
    using System.Diagnostics;
    using System.Reflection;

    public class ApplicationVersion : IApplicationVersion
    {
        /// <inheritdoc />
        public string ProductVersion { get; set; }

        /// <inheritdoc />
        public Version AssemblyVersion { get; set; }

        /// <summary> Creates the version object from assembly. </summary>
        /// <param name="assembly"> The assembly. Default: EntryAssembly </param>
        /// <returns> The version, or null if assembly parameter is null and no entry assembly exists. </returns>
        public static ApplicationVersion Create(Assembly assembly = null)
        {
            assembly??=Assembly.GetEntryAssembly() ?? Assembly.GetCallingAssembly();

            var assemblyLocation = assembly.Location;
            var ver = FileVersionInfo.GetVersionInfo(assemblyLocation).ProductVersion;

            return new ApplicationVersion
                   {
                           ProductVersion = ver,
                           AssemblyVersion = assembly.GetName().Version
                   };
        }
    }
}