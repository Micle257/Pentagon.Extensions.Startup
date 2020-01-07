// -----------------------------------------------------------------------
//  <copyright file="ApplicationArguments.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Startup
{
    using System;

    public class ApplicationArguments : IApplicationArguments
    {
        public ApplicationArguments(string[] arguments)
        {
            Arguments = arguments ?? Array.Empty<string>();
        }

        /// <inheritdoc />
        public string[] Arguments { get; }
    }
}