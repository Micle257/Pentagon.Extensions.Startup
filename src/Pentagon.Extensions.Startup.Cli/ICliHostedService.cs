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

    public interface ICliHostedService
    {
        int? ResultCode { get; }

        Task ExecuteAsync(CancellationToken cancellationToken);
    }
}