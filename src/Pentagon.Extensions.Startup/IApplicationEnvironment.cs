// -----------------------------------------------------------------------
//  <copyright file="IApplicationEnvironment.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Startup
{
    public interface IApplicationEnvironment
    {
        string EnvironmentName { get; }

        string ApplicationName { get; }

        string ContentRootPath {get;}
    }
}