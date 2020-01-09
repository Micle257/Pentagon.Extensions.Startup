using System;

namespace Pentagon.Extensions.Startup.CLI.TestApp
{
    using System.Collections.Generic;
    using System.CommandLine;
    using System.CommandLine.Binding;
    using System.CommandLine.Invocation;
    using System.Linq;
    using System.Net;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Cli;
    using Console;
    using Console.Cli;
    using Logging;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Serilog;
    using Serilog.Debugging;
    using Threading;
    using Console = System.Console;

    class Program
    {
        static Task<int> Main(string[] args)
        {
            var host = CliHostBuilder.GetDefault(args)
                                     .UseCliSerilogLogging()
                                                .Build();

            return host.RunCliAsync();
        }
    }
}
