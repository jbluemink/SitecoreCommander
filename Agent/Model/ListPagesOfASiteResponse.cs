using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SitecoreCommander.Agent.Model
{
    internal class ListPagesOfASiteResponse : BaseAgentResponse
    {
        [JsonPropertyName("items")]
        public List<ListPagesItem> Items { get; set; } = new();
    }

    public class ListPagesItem
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("path")]
        public string Path { get; set; } = string.Empty;
    }
}
