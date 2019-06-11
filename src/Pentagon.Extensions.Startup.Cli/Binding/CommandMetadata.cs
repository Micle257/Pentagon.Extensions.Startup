// -----------------------------------------------------------------------
//  <copyright file="SubverbHelper.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Startup.Cli
{
    using System;
    using System.CommandLine;

    public class CommandMetadata
    {
        public CommandAttribute Attribute { get; set; }

        public Type Type { get; set; }

        public Command Command { get; set; }
    }
}