using System;
using System.Collections.Generic;
using HtmlAgilityPack;
using SitecoreCommander.WordPress.Model;

public class WordPressGalleryRenderer
{
    public class GalleryImage
    { 
        public string? Src { get; set; } // URL van de afbeelding
        public string? Alt { get; set; } // Alt-tekst van de afbeelding
        public int MediaId { get; set; } // Media ID als beschikbaar
    }

    public class ParsedGallery
    {
        public List<GalleryImage> Images { get; set; } = new(); // Lijst van afbeeldingen in de galerij
        public string? LinkTo { get; set; } // Optionele link-instelling van de galerij
    }

    public ParsedGallery Render(WordPressBlock block)
    {
        if (block.BlockType != "gallery")
        {
            throw new InvalidOperationException("Block is not a gallery.");
        }

        var gallery = new ParsedGallery();

        // Parse de AttributesJson voor galerijinstellingen
        if (!string.IsNullOrEmpty(block.AttributesJson))
        {
            var attributes = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(block.AttributesJson);
            if (attributes != null && attributes.TryGetValue("linkTo", out var linkTo))
            {
                gallery.LinkTo = linkTo?.ToString();
            }
        }

        // Parse de InnerHtml voor afbeeldingen
        var doc = new HtmlDocument();
        doc.LoadHtml(block.InnerHtml);

        var imageNodes = doc.DocumentNode.SelectNodes("//img");
        if (imageNodes != null)
        {
            foreach (var imgNode in imageNodes)
            {
                var src = imgNode.GetAttributeValue("src", null);
                var alt = imgNode.GetAttributeValue("alt", null);
                var classAttr = imgNode.GetAttributeValue("class", "");
                int mediaId = -1;

                // Probeer de Media ID te extraheren uit de class-attribuut (bijv. "wp-image-39553")
                var wpImageIdAttr = classAttr.Split(' ').FirstOrDefault(c => c.StartsWith("wp-image-"));
                if (wpImageIdAttr != null && int.TryParse(wpImageIdAttr.Replace("wp-image-", ""), out var parsedMediaId))
                {
                    mediaId = parsedMediaId;
                }

                gallery.Images.Add(new GalleryImage
                {
                    Src = src,
                    Alt = alt,
                    MediaId = mediaId
                });
            }
        }

        return gallery;
    }
}
