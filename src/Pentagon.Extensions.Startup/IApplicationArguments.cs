// -----------------------------------------------------------------------
//  <copyright file="IApplicationArguments.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Startup
{
    using JetBrains.Annotations;

    /// <summary> Represents an entry command line arguments supplied to the program. </summary>
    public interface IApplicationArguments
    {
        /// <summary> Gets the arguments. </summary>
        /// <value> The string array. </value>
        [NotNull]
        string[] Arguments { get; }
    }
}