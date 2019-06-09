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
    using Microsoft.Extensions.Options;
    using Console = System.Console;

    class App : CliApp
    {
        /// <inheritdoc />
        protected override void BuildApp(IApplicationBuilder appBuilder, string[] args)
        {
            base.BuildApp(appBuilder, args);

            appBuilder.AddJsonFileConfiguration(false);

            appBuilder.Services
                      .Configure<LolOptions>(Configuration.GetSection("O"))
                      .Configure<LolOptions>("JSON", Configuration.GetSection("O")); ;

            appBuilder.AddCliOptions<LolOptions>((original, cli) =>
                                                 {
                                                     if (cli.Lol != null)
                                                         original.Lol = cli.Lol;
                                                 });
        }
    }

    public class LolOptions
    {
        public string Lol { get; set; }
    }

    class Program
    {
        public static App App { get; private set; }

        static async Task<int> Main(string[] args)
        {
            App = AppCore.New(new App(), args);

            var res = await App.ExecuteCliAsync(args).ConfigureAwait(false);

            App.OnExit(res == 0);

            return res;
        }
    }

    [Verb("first")]
    public class FirstOptions
    {
        [Option('t')]
        public string Text { get; set; }
    }

   public class FirstCommand : CliHandler<FirstOptions>
    {
        readonly IOptionsMonitor<LolOptions> _optionsMonitor;

        public FirstCommand(IOptionsMonitor<LolOptions> optionsMonitor)
        {
            _optionsMonitor = optionsMonitor;

            _optionsMonitor.OnChange((lolOptions, name) =>
                                     {
                                         Console.WriteLine($"Changed: '{name}' value: {lolOptions.Lol ?? "null"}");
                                     });
        }

        /// <inheritdoc />
        public override async Task<int> RunAsync(FirstOptions options, CancellationToken cancellationToken = default)
        {
            Program.App.UpdateOptions<LolOptions>(o => { o.Lol = "first"; });
            Program.App.UpdateOptions<LolOptions>(o => { o.Lol = "second"; });
            Program.App.UpdateOptions<LolOptions>(o => { o.Lol = null; });

            await Task.Delay(Timeout.InfiniteTimeSpan);

            return 1;
        }
    }
}
