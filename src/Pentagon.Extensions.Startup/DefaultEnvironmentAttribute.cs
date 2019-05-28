// -----------------------------------------------------------------------
//  <copyright file="DefaultEnvironmentAttribute.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Startup
{
    using System;

    [AttributeUsage(AttributeTargets.Assembly)]
    public sealed class DefaultEnvironmentAttribute : Attribute
    {
        /// <summary> Initializes a new instance of the <see cref="DefaultEnvironmentAttribute" /> class. </summary>
        /// <param name="defaultEnvironmentName"> Name of the default environment to be used when no environment variable sets an environment name. </param>
        public DefaultEnvironmentAttribute(string defaultEnvironmentName)
        {
            DefaultEnvironmentName = defaultEnvironmentName;
        }

        /// <summary> Gets the name of the default environment. </summary>
        /// <value> The name of the default environment. </value>
        public string DefaultEnvironmentName { get; }
    }
}