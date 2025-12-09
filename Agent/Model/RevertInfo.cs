using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SitecoreCommander.Agent.Model
{
    internal class RevertInfo
    {
        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("at")]
        public string? At { get; set; }

        [JsonPropertyName("error")]
        public string? Error { get; set; }
    }
}
