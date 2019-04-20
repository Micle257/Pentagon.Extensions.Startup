// -----------------------------------------------------------------------
//  <copyright file="CommandOptions.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Startup.Cli
{
    using System.Threading.Tasks;

    /// <summary>
    /// Represents options for an <see cref="ICliCommand{TOptions}"/>. Uses inverted execution order of command (for command line parsing library).
    /// </summary>
    /// <typeparam name="TCommand">The type of the command.</typeparam>
    /// <typeparam name="TOptions">The type of the options.</typeparam>
    public abstract class CommandOptions<TCommand, TOptions>
            where TCommand : ICliCommand<TOptions>
    {
        /// <summary>
        /// Gets the options.
        /// </summary>
        /// <value>
        /// The options.
        /// </value>
        protected abstract TOptions Options { get; }

        /// <summary>
        /// Called when execution of the command is request. 
        /// </summary>
        /// <returns></returns>
        protected virtual async Task OnExecuteAsync()
        {
            if (DICore.App is CliApp app)
                await app.RunCommand<TCommand, TOptions>(Options, app.FailCallback).ConfigureAwait(false);
        }
    }
}