using System;
using System.Text.Json.Serialization;

namespace SitecoreCommander.Agent.Model
{
    internal class RevertJobRespons
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("tenantId")]
        public string TenantId { get; set; } = string.Empty;

        [JsonPropertyName("jobId")]
        public string JobId { get; set; } = string.Empty;

        [JsonPropertyName("createdAt")]
        public string CreatedAt { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("lastSequenceNumber")]
        public int LastSequenceNumber { get; set; }

        [JsonPropertyName("revert")]
        public RevertInfo? Revert { get; set; }
    }
}
