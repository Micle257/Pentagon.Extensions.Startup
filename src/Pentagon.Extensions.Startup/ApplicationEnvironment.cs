// -----------------------------------------------------------------------
//  <copyright file="ApplicationEnvironment.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Startup
{
    public class ApplicationEnvironment : IApplicationEnvironment
    {
        internal ApplicationEnvironment(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}