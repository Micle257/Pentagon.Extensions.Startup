// -----------------------------------------------------------------------
//  <copyright file="ICliCommand.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Startup.Cli
{
    using System.CommandLine.Invocation;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary> Represents a Command Line command. </summary>
    /// <typeparam name="TOptions"> The type of the options. </typeparam>
    public interface ICliHandler<in TOptions> : ICommandHandler
    {
        Task<int> RunAsync(TOptions options, CancellationToken cancellationToken = default);
    }
}