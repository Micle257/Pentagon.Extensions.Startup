// -----------------------------------------------------------------------
//  <copyright file="ICliHostedService.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Startup.Cli
{
    using System;
    using System.CommandLine;
    using System.Threading;
    using System.Threading.Tasks;
    using JetBrains.Annotations;
    using Microsoft.Extensions.Hosting;

    public interface ICliHostedService : IHostedService
    {
        int? ResultCode { get; }

        RootCommand RootCommand { get; }

        void UpdateOptions<TOptions>([CanBeNull] Action<TOptions> updateCallback);

        void UpdateOptions<TOptions>(string name, [CanBeNull] Action<TOptions> updateCallback);

        void ConfigureCli(Action<RootCommand> callback);
    }
}