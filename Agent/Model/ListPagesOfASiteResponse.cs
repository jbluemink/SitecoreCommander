using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SitecoreCommander.Agent.Model
{
    internal class ListPagesOfASiteResponse : BaseAgentResponse
    {
        public List<ListPagesItem> Items { get; set; } = new();
    }

    public class ListPagesItem
    {
        public string Id { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
    }
}
