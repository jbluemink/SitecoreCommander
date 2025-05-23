namespace SitecoreCommander.WordPress.Model
{
    public class WordPressBlock
    {
        public required string BlockType { get; set; }
        public string? AttributesJson { get; set; }
        public required string InnerHtml { get; set; }

        public int Column { get; set; } = 0;
        public int TotalColumns { get; set; } = 0;
    }
}