using System.Globalization;

namespace SitecoreCommander.Authoring.Model
{
    public class ResultItemWithLayouts
    {
        public int version { get; set; }
        public Language language { get; set; } = null!;
        public string itemId { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
        public string path { get; set; } = string.Empty;
        public ResultTemplateInfo? template { get; set; }
        public ResultValue sharedLayout { get; set; } = null!;
        public ResultValue finalLayout { get; set; } = null!;

        public string? templateId => template?.templateId;
        public string? templateName => template?.name;

        public string itemIdEnclosedInBraces
        {
            get { return Guid.Parse(itemId).ToString("B").ToUpper(); }
        }
    }

    public class ResultTemplateInfo
    {
        public string? templateId { get; set; }
        public string? name { get; set; }
    }
}
