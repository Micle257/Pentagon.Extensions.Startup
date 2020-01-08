namespace Pentagon.Extensions.Startup.Cli {
    using System;

    public class ConsoleProgramCancellationOptions
    {
        public Func<ConsoleKeyInfo, bool> KeyPredicate { get; set; }
    }
}