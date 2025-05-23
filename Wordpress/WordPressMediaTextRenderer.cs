using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using SitecoreCommander.WordPress.Model;
public class WordPressMediaTextRenderer
{
    public Func<int, string, string, string, string, EnvironmentConfiguration, string?>? GetOrCreateMediaDelegate { get; set; }
    public required string mediaFolder { get; set; }
    public required string language { get; set; }
    public required EnvironmentConfiguration env { get; set; }

    public string Render(WordPressBlock block)
    {
        if (block.BlockType != "media-text")
        {
            throw new InvalidOperationException("Block is not a media-text block.");
        }

        var doc = new HtmlDocument();
        doc.LoadHtml(block.InnerHtml);

        var rootNode = doc.DocumentNode.SelectSingleNode("//div[contains(@class, 'wp-block-media-text')]");
        if (rootNode == null)
        {
            throw new InvalidOperationException("Invalid media-text block structure.");
        }

        var mediaHtml = RenderMedia(rootNode.SelectSingleNode(".//figure[contains(@class, 'wp-block-media-text__media')]"));
        var contentHtml = RenderContent(rootNode.SelectSingleNode(".//div[contains(@class, 'wp-block-media-text__content')]"));

        var richText = new StringBuilder();
        richText.Append("<div class=\"media-text\">");
        if (!string.IsNullOrEmpty(mediaHtml))
        {
            richText.Append($"<div class=\"media\">{mediaHtml}</div>");
        }
        if (!string.IsNullOrEmpty(contentHtml))
        {
            richText.Append($"<div class=\"content\">{contentHtml}</div>");
        }
        richText.Append("</div>");

        return richText.ToString();
    }

    private string RenderMedia(HtmlNode? mediaNode)
    {
        if (mediaNode == null)
            return string.Empty;

        var imgNode = mediaNode.SelectSingleNode(".//img");
        if (imgNode == null)
            return string.Empty;

        var src = imgNode.GetAttributeValue("src", null);
        var alt = imgNode.GetAttributeValue("alt", null);
        var classAttr = imgNode.GetAttributeValue("class", "");
        int? mediaId = null;

        // Extract Media ID from class attribute (e.g., "wp-image-211369")
        var wpImageIdAttr = classAttr.Split(' ').FirstOrDefault(c => c.StartsWith("wp-image-"));
        if (wpImageIdAttr != null && int.TryParse(wpImageIdAttr.Replace("wp-image-", ""), out var parsedMediaId))
        {
            mediaId = parsedMediaId;
        }

        string? sitecoreMediaUrl = null;
        if (mediaId.HasValue && GetOrCreateMediaDelegate != null)
        {
            var sitecoreMediaId = GetOrCreateMediaDelegate(mediaId.Value, "MediaText", "", mediaFolder, language, env);
            if (!string.IsNullOrEmpty(sitecoreMediaId))
            {
                // Clean up Sitecore Media ID and generate URL
                var cleaned = Regex.Replace(sitecoreMediaId, "[{}-]", "").ToLowerInvariant();
                sitecoreMediaUrl = $"-/media/{cleaned}.ashx";
            }
        }

        if (!string.IsNullOrEmpty(sitecoreMediaUrl))
        {
            return $"<img src=\"{sitecoreMediaUrl}\" alt=\"{alt ?? ""}\" />";
        }
        else if (!string.IsNullOrEmpty(src))
        {
            return $"<img src=\"{src}\" alt=\"{alt ?? ""}\" />";
        }

        return string.Empty;
    }

    private string RenderContent(HtmlNode? contentNode)
    {
        if (contentNode == null)
            return string.Empty;

        var html = new StringBuilder();

        foreach (var childNode in contentNode.ChildNodes)
        {
            if (childNode.NodeType == HtmlNodeType.Element)
            {
                switch (childNode.Name.ToLower())
                {
                    case "p":
                        html.AppendLine($"<p>{childNode.InnerHtml.Trim()}</p>");
                        break;

                    case "h1":
                    case "h2":
                    case "h3":
                    case "h4":
                    case "h5":
                    case "h6":
                        html.AppendLine($"<{childNode.Name}>{childNode.InnerText.Trim()}</{childNode.Name}>");
                        break;

                    case "a":
                        var href = childNode.GetAttributeValue("href", null);
                        var linkText = childNode.InnerText.Trim();
                        if (!string.IsNullOrEmpty(href))
                        {
                            html.AppendLine($"<a href=\"{href}\">{linkText}</a>");
                        }
                        break;

                    default:
                        Console.WriteLine($"WordPressMediaTextRenderer: Unsupported content element '{childNode.Name}'");
                        break;
                }
            }
        }

        return html.ToString();
    }
}
