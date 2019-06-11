namespace Pentagon.Extensions.Startup.Cli {
    using System;
    using System.CommandLine;
    using JetBrains.Annotations;

    public static class CommandExtensions
    {
        public static Command AddHandler<TCommand>([NotNull] this Command command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            command.Handler = DICore.Get<ICliHandler<TCommand>>();

            return command;
        }
    }
}