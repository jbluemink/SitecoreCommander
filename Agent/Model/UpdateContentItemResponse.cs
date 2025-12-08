using System;
using System.Collections.Generic;

namespace SitecoreCommander.Agent.Model
{
    internal class UpdateContentItemResponse : BaseAgentResponse
    {
        public string itemId { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
        public string path { get; set; } = string.Empty;
        public Dictionary<string, string> updatedFields { get; set; } = new();
    }
}
