// -----------------------------------------------------------------------
//  <copyright file="CliHostBuilder.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Startup.Cli {
    using System;
    using Console.Cli;
    using Microsoft.Extensions.Hosting;

    public static class CliHostBuilder
    {
        public static IHostBuilder GetDefault(string[] args, Action<CliOptions> configure = null) => Host.CreateDefaultBuilder(args).UseCliApp(args, configure);
    }
}