namespace Pentagon.Extensions.Startup.CLI.TestApp {
    using System;
    using Cli;

    [Command("seconds")]
    public class secondsOptions
    {
        [Options("--text", Aliases = new[] { "-t" })]
        public string Text { get; set; }
    }

    [Command("first")]
    public class FirstOptions
    {
    }

    [Command("lol", Parent = typeof(FirstOptions))]
    public class LolCommand
    {
    }

    [Command("lol2", Parent = typeof(LolCommand))]
    public class Lol2Command
    {
        [Options("--text", Aliases = new[] { "-t" })]
        public string Text { get; set; }
    }
}