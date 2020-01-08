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
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Threading;
    using Console = System.Console;

    class Program
    {
        static Task<int> Main(string[] args)
        {
            return CreateHostBuilder(args).Build().RunCliAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
                Host.CreateDefaultBuilder(args: args)
                   //.UseConsoleProgramCancellation(info => info.Key == ConsoleKey.B)
                  .UseCliApp(args)
               //    .ConfigureServices((context,services) =>
               //                       {
               //                           services.Configure<LolOptions>(context.Configuration.GetSection("O"))
               //                                   .Configure<LolOptions>("JSON", context.Configuration.GetSection("O"))
               //                                   .AddCliOptions<LolOptions>((original, cli) =>
               //                                                              {
               //                                                                  if (cli.Lol != null)
               //                                                                      original.Lol = cli.Lol;
               //                                                              });
               //                       })
        ;
    }
}
