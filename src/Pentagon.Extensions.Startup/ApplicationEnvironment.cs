// -----------------------------------------------------------------------
//  <copyright file="ApplicationEnvironment.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Startup
{
    public class ApplicationEnvironment : IApplicationEnvironment
    {
        /// <inheritdoc />
        public string EnvironmentName { get; set; }

        /// <inheritdoc />
        public string ApplicationName { get; set; }

        /// <inheritdoc />
        public string ContentRootPath { get; set; }
    }
}