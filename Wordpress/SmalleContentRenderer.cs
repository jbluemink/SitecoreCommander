using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using HtmlAgilityPack;
using SitecoreCommander.WordPress;
using SitecoreCommander.WordPress.Model;
using WordPressXmlImport;

public class SmalleContentRenderer
{
    public Func<int, string, string, string, string, EnvironmentConfiguration, string?>? GetOrCreateMediaDelegate { get; set; }
    public required string mediaFolder { get; set; }
    public required string language { get; set; }
    public required EnvironmentConfiguration env { get; set; }

    public string Render(WordPressBlock block)
    {
        var parsedBlocks = ParseInnerHtml(block.InnerHtml);
        return RenderBlocks(parsedBlocks);
    }

    private List<SmalleContentBlock> ParseInnerHtml(string html)
    {
        var blocks = new List<SmalleContentBlock>();

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var rootNode = doc.DocumentNode;

        // Als de root alleen een div is, pak dan de kinderen
        if (rootNode.FirstChild?.Name == "div" &&
            rootNode.FirstChild.GetAttributeValue("class", "").Contains("wp-block-sogutenberg-smallecontent"))
        {
            rootNode = rootNode.FirstChild;
        }

        foreach (var node in rootNode.ChildNodes)
        {
            if (node.NodeType == HtmlNodeType.Comment)
            {
                // Sla comments over zoals <!-- wp:paragraph -->
                continue;
            }

            if (node.NodeType != HtmlNodeType.Element)
                continue;

            blocks.AddRange(ProcessNode(node));
        }

        return blocks;
    }

    private List<SmalleContentBlock> ProcessNode(HtmlNode node)
    {
        var blocks = new List<SmalleContentBlock>();

        switch (node.Name.ToLower())
        {
            case "h1":
            case "h2":
            case "h3":
            case "h4":
            case "h5":
            case "h6":
                if (int.TryParse(node.Name.Substring(1), out int level))
                {
                    blocks.Add(new SmalleContentBlock
                    {
                        Type = "heading",
                        Level = level,
                        Text = node.InnerText.Trim()
                    });
                }
                break;

            case "p":
                blocks.Add(new SmalleContentBlock
                {
                    Type = "paragraph",
                    Text = node.InnerHtml.Trim() // InnerHtml zodat je <strong> en andere inline tags behoudt
                });
                break;

            case "ul":
                var ulItems = new List<string>();
                foreach (var li in node.SelectNodes("./li") ?? new HtmlNodeCollection(node))
                {
                    ulItems.Add(li.InnerHtml.Trim());
                }
                blocks.Add(new SmalleContentBlock
                {
                    Type = "list",
                    ListItems = ulItems
                });
                break;

            case "div":
                // div behandelen alsof het gewoon doorgegeven content is
                foreach (var child in node.ChildNodes)
                {
                    if (child.NodeType == HtmlNodeType.Element)
                    {
                        blocks.AddRange(ProcessNode(child));
                    }
                }
                break;

            case "blockquote":
                blocks.Add(new SmalleContentBlock
                {
                    Type = "blockquote",
                    Text = node.InnerText.Trim()
                });
                break;

            case "a":
                string? href = node.GetAttributeValue("href", null);
                string linkText = node.InnerText.Trim();
                if (!string.IsNullOrEmpty(href))
                {
                    blocks.Add(new SmalleContentBlock
                    {
                        Type = "link",
                        Text = linkText,
                        Href = href
                    });
                }
                break;

            case "i":
                blocks.Add(new SmalleContentBlock
                {
                    Type = "italic",
                    Text = node.InnerText.Trim()
                });
                break;

            case "figure":
                var figureimgNode = node.Descendants("img").FirstOrDefault();
                if (figureimgNode != null)
                {
                    var wpImageIdAttr = figureimgNode.GetAttributeValue("class", "") // zoek bv. "wp-image-212239"
                        .Split(' ')
                        .FirstOrDefault(c => c.StartsWith("wp-image-"));

                    if (wpImageIdAttr != null && int.TryParse(wpImageIdAttr.Replace("wp-image-", ""), out var mediaIntId))
                    {
                        blocks.Add(new SmalleContentBlock
                        {
                            Type = "image",
                            MediaId = mediaIntId.ToString(),
                            Alt = figureimgNode.GetAttributeValue("alt", "")
                        });
                    }
                }
                break;

            case "img":
                var imgNode = node.Name == "img" ? node : node.SelectSingleNode(".//img");
                if (imgNode != null)
                {
                    string? src = imgNode.GetAttributeValue("src", null);
                    string? alt = imgNode.GetAttributeValue("alt", null);

                    blocks.Add(new SmalleContentBlock
                    {
                        Type = "image",
                        Src = src,
                        Alt = alt,
                        MediaId = ExtractMediaIdFromSrc(src)
                    });
                }
                break;

            default:
                // fallback: andere elementen negeren
                Console.WriteLine("Unknow node  ProcessNode :"+ node.Name);
                break;
        }

        return blocks;
    }

    private string RenderBlocks(List<SmalleContentBlock> blocks)
    {
        var html = new StringBuilder();

        foreach (var block in blocks)
        {
            switch (block.Type)
            {
                case "heading":
                    html.AppendLine($"<h{block.Level}>{block.Text}</h{block.Level}>");
                    break;

                case "paragraph":
                    html.AppendLine($"<p>{block.Text}</p>");
                    break;

                case "italic":
                    html.AppendLine($"<i>{block.Text}</i>");
                    break;

                case "blockquote":
                    html.AppendLine($"<blockquote>{block.Text}</blockquote>");
                    break;

                case "list":
                    html.AppendLine("<ul>");
                    foreach (var item in block.ListItems ?? new List<string>())
                    {
                        html.AppendLine($"<li>{item}</li>");
                    }
                    html.AppendLine("</ul>");
                    break;

                case "link":
                    if (!string.IsNullOrEmpty(block.Href))
                    {
                        html.AppendLine($"<a href=\"{block.Href}\">{block.Text}</a>");
                    }
                    break;

                case "image":
                    if (!string.IsNullOrEmpty(block.MediaId))
                    {
                        string mediaUrl = GetOrCreateSitecoreMedia(block.MediaId);
                        html.AppendLine($"<figure class=\"image\"><img src=\"{mediaUrl}\" alt=\"{block.Alt ?? ""}\" /></figure>");
                    }
                    else if (!string.IsNullOrEmpty(block.Src))
                    {
                        html.AppendLine($"<figure class=\"image\"><img src=\"{block.Src}\" alt=\"{block.Alt ?? ""}\" /></figure>");
                    }
                    break;
                default:
                    {
                        Console.WriteLine($"SmalleContentRenderer Unknown block type: {block.Type}");
                        break;
                    }
            }
        }

        return html.ToString();
    }

    private string GetOrCreateSitecoreMedia(string mediaId)
    {
        if (string.IsNullOrEmpty(mediaId))
            return string.Empty;

        // Ensure GetOrCreateMediaDelegate is not null before invoking it
        if (GetOrCreateMediaDelegate == null)
            throw new InvalidOperationException("GetOrCreateMediaDelegate is not set.");

        var sitecoreMediaId = GetOrCreateMediaDelegate(int.Parse(mediaId), "Images", "", mediaFolder, language, env);

        if (string.IsNullOrEmpty(sitecoreMediaId))
            return string.Empty;

        // Haal {} en - weg en lowercase
        var cleaned = Regex.Replace(sitecoreMediaId, "[{}-]", "").ToLowerInvariant();

        return $"-/media/{cleaned}.ashx";
    }

    private string? ExtractMediaIdFromSrc(string? src)
    {
        if (string.IsNullOrEmpty(src))
            return null;

        // Simpel voorbeeld: als de bron een GUID bevat, probeer hem eruit te halen
        var match = Regex.Match(src, @"[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}");
        if (match.Success)
        {
            return match.Value;
        }

        return null;
    }
}
