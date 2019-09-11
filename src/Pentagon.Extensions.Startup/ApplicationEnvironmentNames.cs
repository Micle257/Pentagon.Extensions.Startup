// -----------------------------------------------------------------------
//  <copyright file="ApplicationEnvironmentNames.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Startup
{
    using System;
    using System.Collections.Generic;
    using JetBrains.Annotations;

    public static class ApplicationEnvironmentNames
    {
        public const string Development = "Development";

        public const string Production = "Production";

        public const string Staging = "Staging";

        [NotNull]
        internal static HashSet<string> Defined = new HashSet<string>(new[] {Development, Production, Staging}, StringComparer.InvariantCultureIgnoreCase);

        /// <summary>
        /// Defines the specified environment.
        /// </summary>
        /// <param name="name">The name.</param>
        public static void Define(string name)
        {
            Defined.Add(name);
        }
    }
}