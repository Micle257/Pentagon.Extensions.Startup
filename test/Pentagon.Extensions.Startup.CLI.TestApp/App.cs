namespace Pentagon.Extensions.Startup.CLI.TestApp {
    using System.Threading;
    using Cli;
    using Logging.ColoredConsole;
    using Microsoft.Extensions.DependencyInjection;

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

            appBuilder.AddLogging(a => a.AddColoredConsole());
        }
    }
}