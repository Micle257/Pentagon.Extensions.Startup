namespace Pentagon.Extensions.Startup.CLI.TestApp {
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Cli;
    using Console.Cli;
    using Microsoft.Extensions.Options;

    [CliCommand("first")]
    public class FirstOptions
    {
        [CliOption("--text",  "-t" )]
        public string Text { get; set; }

        [CliArgument(IsRequired =  false)]
        public string Other { get; set; }

        public class Handler : ICliCommandHandler<FirstOptions>
        {
            readonly IOptionsMonitor<LolOptions> _optionsMonitor;
            readonly ICliOptionsUpdateService _updateService;

            public Handler(IOptionsMonitor<LolOptions> optionsMonitor,
                           ICliOptionsUpdateService updateService)
            {
                _optionsMonitor = optionsMonitor;
                _updateService = updateService;

                _optionsMonitor.OnChange((lolOptions, name) =>
                                         {
                                             Console.WriteLine($"Changed: '{name}' value: {lolOptions.Lol ?? "null"}");
                                         });
            }

            /// <inheritdoc />
            public async Task<int> ExecuteAsync(FirstOptions command, CancellationToken cancellationToken)
            {
                _updateService.UpdateOptions<LolOptions>(o => { o.Lol = "first"; });
                _updateService.UpdateOptions<LolOptions>(o => { o.Lol = "second"; });
                _updateService.UpdateOptions<LolOptions>(o => { o.Lol = null; });

                try
                {
                    await Task.Delay(3000, cancellationToken);

                    return 1;
                }
                catch (OperationCanceledException e)
                {
                    Console.WriteLine("Wait bitch");
                    await Task.Delay(TimeSpan.FromSeconds(1));
                    Console.WriteLine("good");
                    return 2;
                }

                return 0;
            }
        }
    }

}