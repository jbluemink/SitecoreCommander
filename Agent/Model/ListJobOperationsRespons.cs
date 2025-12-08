using System;
using System.Collections.Generic;

namespace SitecoreCommander.Agent.Model
{
    internal class ListJobOperationsResponse
    {
        public string id { get; set; } = string.Empty;
        public string type { get; set; } = string.Empty;
        public string tenantId { get; set; } = string.Empty;
        public string jobId { get; set; } = string.Empty;
        public int sequenceNumber { get; set; } = 0;
        public string timestamp { get; set; } = string.Empty;
        public string operation { get; set; } = string.Empty;
        public string effectType { get; set; } = string.Empty;
        public string item { get; set; } = string.Empty;
        public string revert { get; set; } = string.Empty;

    }
}
