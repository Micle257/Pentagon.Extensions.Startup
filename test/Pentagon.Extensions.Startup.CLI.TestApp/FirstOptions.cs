namespace Pentagon.Extensions.Startup.CLI.TestApp {
    using System;
    using Cli;

    [Command("first")]
    public class FirstOptions
    {
        [Options("--text", Aliases = new[] { "-t" })]
        public string Text { get; set; }

        [CliArgument()]
        public string Other { get; set; }
    }

}