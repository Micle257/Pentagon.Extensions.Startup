// -----------------------------------------------------------------------
//  <copyright file="ICliCommand.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Startup.Cli
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary> Represents a Command Line command. </summary>
    /// <typeparam name="TOptions"> The type of the options. </typeparam>
    public interface ICliHandler<in TOptions>
    {
        Task<int> RunAsync(TOptions options, CancellationToken cancellationToken = default);
    }

    public abstract class CliHandler<TOptions> : ICliHandler<TOptions>
    {
        public virtual void BuildApp(TOptions options) { }

        /// <inheritdoc />
        public abstract Task<int> RunAsync(TOptions options, CancellationToken cancellationToken = default);
    }
}