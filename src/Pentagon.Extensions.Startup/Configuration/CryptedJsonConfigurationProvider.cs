// -----------------------------------------------------------------------
//  <copyright file="CryptedJsonConfigurationProvider.cs">
//   Copyright (c) Michal Pokorný. All Rights Reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Pentagon.Extensions.Startup.Configuration
{
    using System.IO;
    using Microsoft.Extensions.Configuration.Json;

    public class CryptedJsonConfigurationProvider : JsonConfigurationProvider
    {
        /// <inheritdoc />
        public CryptedJsonConfigurationProvider(CryptedJsonConfigurationSource source) : base(source) { }

        /// <inheritdoc />
        public override void Load(Stream stream)
        {
            // decrypt stream

            base.Load(stream);
        }
    }
}