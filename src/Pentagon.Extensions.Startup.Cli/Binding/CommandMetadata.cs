// -----------------------------------------------------------------------
//  <copyright file="SubverbHelper.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Startup.Cli
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.CommandLine;
    using System.Reflection;

    public class CommandMetadata
    {
        public CliCommandAttribute Attribute { get; set; }

        public Type Type { get; set; }

        public Command Command { get; set; }

        public IReadOnlyList<OptionMetadata> Options { get; set; }

        public IReadOnlyList<ArgumentMetadata> Arguments { get; set; }
    }

    public class ArgumentMetadata
    {
        public PropertyInfo PropertyInfo { get; set; }

        public CliArgumentAttribute Attribute { get; set; }
    }

    public class OptionMetadata
    {
        public PropertyInfo PropertyInfo { get; set; }

        public CliOptionAttribute Attribute { get; set; }
    }
}