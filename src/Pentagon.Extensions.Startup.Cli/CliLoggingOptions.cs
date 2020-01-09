// -----------------------------------------------------------------------
//  <copyright file="CliLoggingOptions.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Startup.Cli
{
    public class CliLoggingOptions
    {
        public bool InternalLogging { get; set; }

        public bool MethodLogging { get; set; }

        public bool RunId { get; set; } = true;

        public bool HostingEnvironment { get; set; } = true;

        public CliLoggingConsoleOptions Console { get; set; }
    }
}