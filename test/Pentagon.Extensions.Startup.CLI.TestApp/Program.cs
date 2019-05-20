using System;

namespace Pentagon.Extensions.Startup.CLI.TestApp
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using Cli;
    using CommandLine;
    using Microsoft.Extensions.DependencyInjection;
    using Console = System.Console;

    class App : CliApp
    {
        /// <inheritdoc />
        protected override void BuildApp(IApplicationBuilder appBuilder, string[] args)
        {
            appBuilder.AddCliCommands();
        }
    }

    class Program
    {
        static async Task<int> Main(string[] args)
        {
            var app = AppCore.New(new App(), args);

            var res = await app.ExecuteCliAsync(args).ConfigureAwait(false);

            app.OnExit(res == 0);

            return res;
        }
    }

    [Verb("first")]
    class FirstOptions
    {
        [Option('t')]
        public string Text { get; set; }
    }

    [Verb("second")]
    class SecondOptions
    {
        [Option('t')]
        public string Text { get; set; }
    }

    [Verb("op")]
    class OpOptions
    {
        [Value(0)]
        public string Text { get; set; }
    }

    [Verb("op2")]
    class Op2Options
    {
        [Value(0)]
        public string Text { get; set; }
    }

    class FirstCommand : ICliCommand<FirstOptions>
    {
        /// <inheritdoc />
        public Task<int> RunAsync(FirstOptions options, CancellationToken cancellationToken = default)
        {
            Console.WriteLine("First");
            Console.WriteLine(options.Text);

            return Task.FromResult(50);
        }
    }
}
