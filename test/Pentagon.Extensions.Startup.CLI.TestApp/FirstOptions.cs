namespace Pentagon.Extensions.Startup.CLI.TestApp {
    using System;
    using Cli;

    [CliCommand("first")]
    public class FirstOptions
    {
        [CliOption("--text", Aliases = new[] { "-t" })]
        public string Text { get; set; }

        [CliArgument()]
        public string Other { get; set; }
    }

}