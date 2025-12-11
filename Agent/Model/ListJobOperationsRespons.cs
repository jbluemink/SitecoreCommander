using System.Text.Json.Serialization;

namespace SitecoreCommander.Agent.Model
{
    internal class ListJobOperationsResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("tenantId")]
        public string TenantId { get; set; } = string.Empty;

        [JsonPropertyName("jobId")]
        public string JobId { get; set; } = string.Empty;

        [JsonPropertyName("sequenceNumber")]
        public int SequenceNumber { get; set; } = 0;

        [JsonPropertyName("timestamp")]
        public string Timestamp { get; set; } = string.Empty;

        [JsonPropertyName("operation")]
        public string Operation { get; set; } = string.Empty;

        [JsonPropertyName("effectType")]
        public string EffectType { get; set; } = string.Empty;

        [JsonPropertyName("item")]
        public ItemInfo? Item { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty;

        [JsonPropertyName("revert")]
        public RevertInfo? Revert { get; set; }
    }

    internal class ItemInfo
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("version")]
        public VersionInfo? Version { get; set; }

        [JsonPropertyName("language")]
        public string Language { get; set; } = string.Empty;

        [JsonPropertyName("archiveId")]
        public string? ArchiveId { get; set; }
    }

    internal class VersionInfo
    {
        [JsonPropertyName("before")]
        public string? Before { get; set; }

        [JsonPropertyName("after")]
        public string? After { get; set; }
    }
}
