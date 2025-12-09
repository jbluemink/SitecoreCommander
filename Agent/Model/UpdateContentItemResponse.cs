using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SitecoreCommander.Agent.Model
{
    internal class UpdateContentItemResponse : BaseAgentResponse
    {
        [JsonPropertyName("itemId")]
        public string ItemId { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("path")]
        public string Path { get; set; } = string.Empty;

        [JsonPropertyName("updatedFields")]
        public Dictionary<string, string> UpdatedFields { get; set; } = new();
    }
}
