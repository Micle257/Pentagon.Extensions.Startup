// -----------------------------------------------------------------------
//  <copyright file="AppConsoleCore.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Startup.Cli
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Utilities.Console.Helpers;

    public abstract class AppConsoleCore : AppCore
    {
        /// <summary>
        /// Gets the fail callback. Default callback is no action.
        /// </summary>
        /// <value>
        /// The <see cref="Func{TResult}"/> with return value of <see cref="Task"/>.
        /// </value>
        public virtual Func<Task> FailCallback { get; protected set; } = () => Task.CompletedTask;

        public virtual void OnExit(bool success)
        {
            if (Environment.IsDevelopment())
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
            else if (!Environment.IsDevelopment() && !success)
            {
                ConsoleHelper.WriteError(errorValue: "Program execution failed.");
                Console.WriteLine();
            }
        }

        public async Task RunCommand<TCommand, TOptions>(TOptions options, Func<Task> failCallback)
                where TCommand : ICliCommand<TOptions>
        {
            try
            {
                var command = Services.GetService<TCommand>();

                await command.RunAsync(options).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Services.GetService<ILogger>()?.LogCritical(e, message: "Error while running play command.");
                await (failCallback?.Invoke()).ConfigureAwait(false);
                throw;
            }
        }
    }
}