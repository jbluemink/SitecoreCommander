using System.Globalization;

namespace SitecoreCommander.Authoring.Model
{
    internal class ResultItemWithField
    {
        public int version { get; set; }
        public Language language { get; set; }
        public string itemId { get; set; }
        public string name { get; set; }
        public string path { get; set; }
        public ResultValue fieldvalue { get; set; }
        public string itemIdEnclosedInBraces
        {
            get { return Guid.Parse(itemId).ToString("B").ToUpper(); }
        }

    }
}
