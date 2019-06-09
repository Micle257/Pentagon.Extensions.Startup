// -----------------------------------------------------------------------
//  <copyright file="CliOptionsDelegate.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Startup.Cli
{
    public delegate void CliOptionsDelegate<TOptions>(TOptions original, TOptions cli);
}