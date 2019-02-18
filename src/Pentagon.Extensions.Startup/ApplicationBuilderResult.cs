// -----------------------------------------------------------------------
//  <copyright file="ApplicationBuilderResult.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Startup
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Logging;
    using Microsoft.Extensions.Logging;

    public class ApplicationBuilderResult
    {
        public ApplicationBuilderResult(IServiceProvider provider, IList<(LogLevel Level, LoggerState State, Exception Exception)> log)
        {
            Provider = provider;
            Log = log;
        }

        public IServiceProvider Provider { get; }

        public IList<(LogLevel Level, LoggerState State, Exception Exception)> Log { get; }

        public void ApplyLogMessages(ILogger logger)
        {
            foreach (var msg in Log)
                logger.Log(msg.Level,
                           new EventId(0),
                           msg.State,
                           msg.Exception,
                           (state, exception) => LoggerSourceFormatter.Format(state.GetRawState(), exception));
        }
    }
}