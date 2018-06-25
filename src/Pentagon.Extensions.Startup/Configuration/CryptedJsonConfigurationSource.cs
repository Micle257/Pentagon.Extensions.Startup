// -----------------------------------------------------------------------
//  <copyright file="CryptedJsonConfigurationSource.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Startup.Configuration
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Configuration.Json;

    public class CryptedJsonConfigurationSource : JsonConfigurationSource
    {
        public override IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            EnsureDefaults(builder);
            return new CryptedJsonConfigurationProvider(this);
        }
    }
}