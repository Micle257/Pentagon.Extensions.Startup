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
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Threading;
    using Console = System.Console;

    class Program
    {
        public static App App { get; private set; }

        static async Task<int> Main(string[] args)
        {
            try
            {
                return await Run(args);
            }
            finally
            {
                await Task.Delay(1500);
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
                Host.CreateDefaultBuilder(args: args)
                    .UseCliApp(args, b => b.RegisterParallelCallback(token => Task.FromResult(1)))
                    .UseConsoleProgramCancellation(info => info.Key == ConsoleKey.B);

        static async Task<int> Run(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            
            var cliapp = host.Services.GetRequiredService<ICliHostedService>();

            cliapp.RegisterCancelKeyHandler(info => info.Key == ConsoleKey.B);

            cliapp.RegisterParallelCallback(async (ct) =>
                                            {
                                                while (!ct.IsCancellationRequested)
                                                {
                                                    try
                                                    {
                                                        using (var client = new WebClient())
                                                        {
                                                            using (await client.OpenReadTaskAsync("http://clients3.google.com/generate_204").TimeoutAfter(TimeSpan.FromSeconds(1)))
                                                            {
                                                                ConsoleWriter.WriteSuccess("Internet available.");
                                                                Console.WriteLine();
                                                            }
                                                        }
                                                    }
                                                    catch
                                                    {
                                                        ConsoleWriter.WriteError("Internet don't.");
                                                        Console.WriteLine();
                                                    }

                                                    Thread.Sleep(500);
                                                }
                                            });

            await host.RunAsync().ConfigureAwait(false);

            return cliapp.ResultCode.GetValueOrDefault();
        }
    }
}
