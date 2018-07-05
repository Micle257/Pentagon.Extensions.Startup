using System;

namespace Pentagon.Extensions.Startup.Cli
{
    using System.Threading.Tasks;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Utilities.Console.Helpers;

    public abstract class AppConsoleCore : AppCore
    {
        public async Task RunCommand<TCommand, TOptions>(TOptions options, Action failCallback)
                where TCommand : ICliCommand<TOptions>
        {
            try
            {
                var command = Services.GetService<TCommand>();

                await command.RunAsync(options);
            }
            catch (Exception e)
            {
                Services.GetService<ILogger>()?.LogCritical(e, message: "Error while running play command.");
                failCallback();
                throw;
            }

        }

        public virtual void OnExit(bool success)
        {
            if (Environment.EnvironmentName == ApplicationEnvironmentNames.Development)
            {
                Console.WriteLine();
                if (!success)
                {
                    ConsoleHelper.WriteError(errorValue: "Program execution failed.");
                    Console.WriteLine();
                }

                Console.WriteLine(value: " Press any key to exit the application...");
                Console.ReadKey();
            }
            else if (Environment.EnvironmentName == ApplicationEnvironmentNames.Production && !success)
            {
                ConsoleHelper.WriteError(errorValue: "Program execution failed.");
                Console.WriteLine();
            }
        }
    }
}
