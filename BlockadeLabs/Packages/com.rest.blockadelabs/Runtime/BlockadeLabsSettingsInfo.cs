// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using Utilities.WebRequestRest.Interfaces;

namespace BlockadeLabs
{
    public sealed class BlockadeLabsSettingsInfo : ISettingsInfo
    {
        internal const string Https = "https://";
        internal const string DefaultDomain = "backend.blockadelabs.com";
        internal const string DefaultVersion = "v1";

        /// <summary>
        /// Creates a new instance of <see cref="BlockadeLabsSettingsInfo"/> for use with BlockadeLabs.
        /// </summary>
        public BlockadeLabsSettingsInfo()
        {
            Domain = DefaultDomain;
            BaseRequest = $"/api/{DefaultVersion}/";
            BaseRequestUrlFormat = $"{Https}{Domain}{BaseRequest}{{0}}";
        }

        /// <summary>
        /// Creates a new instance of <see cref="BlockadeLabsSettingsInfo"/> for use with BlockadeLabs.
        /// </summary>
        /// <param name="domain">Base api domain.</param>
        public BlockadeLabsSettingsInfo(string domain)
        {
            if (string.IsNullOrWhiteSpace(domain))
            {
                domain = DefaultDomain;
            }

            if (!domain.Contains('.') &&
                !domain.Contains(':'))
            {
                throw new ArgumentException($"Invalid parameter \"{nameof(domain)}\"");
            }

            Domain = domain.Contains("http") ? domain : $"{Https}{domain}";
            BaseRequest = $"/api/{DefaultVersion}/";
            BaseRequestUrlFormat = $"{Domain}{BaseRequest}{{0}}";
        }

        public string Domain { get; }

        public string BaseRequest { get; }

        public string BaseRequestUrlFormat { get; }
    }
}
