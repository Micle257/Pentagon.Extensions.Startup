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

        public class Handler : ICliCommandHandler<FirstOptions>, ICliCommandPropertyHandler<FirstOptions>
        {
            readonly ICliOptionsUpdateService _updateService;

            public Handler(
                           ICliOptionsUpdateService updateService)
            {
                _updateService = updateService;
            }

            /// <inheritdoc />
            public async Task<int> ExecuteAsync(FirstOptions command, CancellationToken cancellationToken)
            {
                _updateService.UpdateOptions<FirstOptions>(o => { o.Text = "sdd"; });

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

            /// <inheritdoc />
            public FirstOptions Command { get; }
        }
    }

}