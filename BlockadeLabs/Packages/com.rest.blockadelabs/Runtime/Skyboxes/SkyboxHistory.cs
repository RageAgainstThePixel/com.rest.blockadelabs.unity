// Licensed under the MIT License. See LICENSE in the project root for license information.

using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine.Scripting;

namespace BlockadeLabs.Skyboxes
{
    [Preserve]
    public sealed class SkyboxHistory : BaseResponse, IListResponse<SkyboxInfo>
    {
        [Preserve]
        [JsonConstructor]
        internal SkyboxHistory(
            [JsonProperty("data")] List<SkyboxInfo> skyboxes,
            [JsonProperty("totalCount")] int totalCount,
            [JsonProperty("has_more")] bool hasMore)
        {
            Items = skyboxes;
            TotalCount = totalCount;
            HasMore = hasMore;
        }

        [Preserve]
        [JsonProperty("totalCount")]
        public int TotalCount { get; }

        [Preserve]
        [JsonProperty("has_more")]
        public bool HasMore { get; }

        [Preserve]
        [JsonProperty("data")]
        public IReadOnlyList<SkyboxInfo> Items { get; }

        [Preserve]
        [JsonIgnore]
        public IReadOnlyList<SkyboxInfo> Skyboxes => Items;
    }
}
