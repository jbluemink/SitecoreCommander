using System;
using System.Collections.Generic;

namespace SitecoreCommander.Agent.Model
{
    internal class RetrieveJobDetailsRespons
    {
        public string id { get; set; } = string.Empty;
        public string status { get; set; } = string.Empty;
        public string tenantId { get; set; } = string.Empty;

        public string jobId { get; set; } = string.Empty;
        public string createdAt { get; set; } = string.Empty;
        public string type { get; set; } = string.Empty;
        public int lastSequenceNumber { get; set; } = 0;
        public string revert { get; set; } = string.Empty;

    }
}
