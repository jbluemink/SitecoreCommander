using System;
using System.Collections.Generic;

namespace SitecoreCommander.Agent.Model
{
    internal class ListJobOperationsResponse : BaseAgentResponse
    {
        public string id { get; set; } = string.Empty;
        public string type { get; set; } = string.Empty;
        public string tenantId { get; set; } = string.Empty;
        public string jobId { get; set; } = string.Empty;
        public int sequenceNumber { get; set; } = 0;
        public string timestamp { get; set; } = string.Empty;
        public string operation { get; set; } = string.Empty;
        public string effectType { get; set; } = string.Empty;
        public ItemInfo? item { get; set; }
        public string status { get; set; } = string.Empty;
        public string? revert { get; set; }
    }

    internal class ItemInfo
    {
        public string id { get; set; } = string.Empty;
        public VersionInfo? version { get; set; }
        public string language { get; set; } = string.Empty;
        public string? archiveId { get; set; }
    }

    internal class VersionInfo
    {
        public string? before { get; set; }
        public string? after { get; set; }
    }
}
