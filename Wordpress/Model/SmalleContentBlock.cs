namespace SitecoreCommander.WordPress.Model
{

    public class SmalleContentBlock
    {
        public string Type { get; set; } = string.Empty; // "heading", "paragraph", "image"
        public int? Level { get; set; } // for headings (h2, h3)
        public string? Text { get; set; } 
        public List<string>? ListItems { get; set; }
        public string? Alt { get; set; } // for images
        public string? Src { get; set; }
        public string? MediaId { get; set; }
        public string? Href { get; set; }
    }
}
