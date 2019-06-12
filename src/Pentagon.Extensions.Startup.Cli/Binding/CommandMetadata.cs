// -----------------------------------------------------------------------
//  <copyright file="SubverbHelper.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Startup.Cli
{
    using System;
    using System.Collections.Generic;
    using System.CommandLine;
    using System.Reflection;

    public class CommandMetadata
    {
        public CommandAttribute Attribute { get; set; }

        public Type Type { get; set; }

        public Command Command { get; set; }

        public IReadOnlyList<OptionMetadata> Options { get; set; }
    }

    public class OptionMetadata
    {
        public PropertyInfo PropertyInfo { get; set; }

        public OptionsAttribute Attribute { get; set; }
    }
}