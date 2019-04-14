// -----------------------------------------------------------------------
//  <copyright file="SubVerbAttribute.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Startup.Cli
{
    using System;

    class SubVerbAttribute : Attribute
    {
        public SubVerbAttribute(Type type)
        {
            Type = type;
        }

        public Type Type { get; }
    }
}