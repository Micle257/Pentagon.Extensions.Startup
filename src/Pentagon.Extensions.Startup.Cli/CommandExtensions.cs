namespace Pentagon.Extensions.Startup.Cli {
    using System;
    using System.CommandLine;
    using System.CommandLine.Invocation;
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

        public static Command AddHandler(this Command command, Type type)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            command.Handler = (ICommandHandler) DICore.Get(typeof(ICliHandler<>).MakeGenericType(type));

            return command;
        }
    }
}