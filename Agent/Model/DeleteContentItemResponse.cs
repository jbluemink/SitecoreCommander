using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SitecoreCommander.Agent.Model
{
    internal class DeleteContentItemResponse : BaseAgentResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("deletedId")]
        public string DeletedId { get; set; } = string.Empty;

        //error response
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;
        [JsonPropertyName("detail")]
        public string Detail { get; set; } = string.Empty;
    }
}
