// -----------------------------------------------------------------------
//  <copyright file="SubVerbAttribute.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Startup.Cli
{
    using System;

    public class SubCommandAttribute : Attribute
    {
        public Type Type { get; }

        public SubCommandAttribute(Type type)
        {
            Type = type;
        }
    }
}