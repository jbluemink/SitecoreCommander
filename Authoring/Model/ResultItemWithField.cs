using System.Globalization;

namespace SitecoreCommander.Authoring.Model
{
    public class ResultItemWithField
    {
        public int version { get; set; }
        public Language language { get; set; } = null!;
        public string itemId { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
        public string path { get; set; } = string.Empty;
        public ResultValue fieldvalue { get; set; } = null!;
        public string itemIdEnclosedInBraces
        {
            get { return Guid.Parse(itemId).ToString("B").ToUpper(); }
        }

    }
}
