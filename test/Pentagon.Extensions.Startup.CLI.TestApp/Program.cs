using System;

namespace Pentagon.Extensions.Startup.CLI.TestApp
{
    using System.Collections.Generic;
    using System.CommandLine;
    using System.CommandLine.Binding;
    using System.CommandLine.Invocation;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Cli;

    class Program
    {
        public static App App { get; private set; }

        static async Task<int> Main(string[] args)
        {
            App = AppCore.New(new App(), args);

            App.RegisterCancelKeyHandler(info => info.Key == ConsoleKey.B);

            var res = await App.ExecuteCliAsync(args).ConfigureAwait(false);

            App.OnExit(res == 0);

            return res;
        }
    }
}
