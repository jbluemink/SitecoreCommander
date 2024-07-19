using SitecoreCommander.Command;
using System.IO;

namespace SitecoreCommander.Edge.Model
{
    internal class SearchResultItem
    {
        public string id { get; set; }
        public string path  { get; set; }
        public int version { get; set; }

        public Language language { get; set; }
    }
}
