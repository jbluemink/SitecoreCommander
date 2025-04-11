using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace WordpressXmlImport
{
    public class WpCategory
    {
        public required string Domain { get; set; }
        public required string Nicename { get; set; }
        public required string Name { get; set; }
    }

    public class WordPressPost
    {
        public required int Id { get; set; }
        public required int ParentId { get; set; }
        public required string Link { get; set; }
        public required string Title { get; set; }
        public required string Content { get; set; }
        public required string PostType { get; set; }
        public required DateTime PostDate { get; set; }
        public required string Slug { get; set; }
        public required string Status { get; set; }
        public required string UrlName { get; set; }
        public required List<WpCategory> Categories { get; set; } = new();
        public string? YoastTitle { get; set; }
        public string? YoastMetaDescription { get; set; }
    }

    public class ExtendedXmlImporter
    {
        private static readonly XNamespace contentNs = "http://purl.org/rss/1.0/modules/content/";
        private static readonly XNamespace wpNs = "http://wordpress.org/export/1.2/";
        private static readonly XNamespace dcNs = "http://purl.org/dc/elements/1.1/";

        public List<WordPressPost> LoadPosts(string filePath)
        {
            var doc = XDocument.Load(filePath);

            var posts = doc.Descendants("item")
                .Select(x =>
                {
                    var postMetas = x.Elements(wpNs + "postmeta");

                    string? GetMetaValue(string key) =>
                        postMetas.FirstOrDefault(m => (string?)m.Element(wpNs + "meta_key") == key)?
                                 .Element(wpNs + "meta_value")?.Value;

                    return new WordPressPost
                    {
                        Id = (int?)x.Element(wpNs + "post_id") ?? -1,
                        ParentId = (int?)x.Element(wpNs + "post_parent") ?? -1,
                        Link = (string?)x.Element("link") is string link && Uri.TryCreate(link, UriKind.Absolute, out var uri)
                            ? uri.PathAndQuery
                            : "",
                        Title = (string?)x.Element("title") ?? "",
                        Content = (string?)x.Element(contentNs + "encoded") ?? "",
                        PostType = (string?)x.Element(wpNs + "post_type") ?? "",
                        PostDate = ParseDate((string?)x.Element(wpNs + "post_date") ?? ""),
                        Slug = (string?)x.Element(wpNs + "post_name") ?? "",
                        Status = (string?)x.Element(wpNs + "status") ?? "",
                        UrlName = (string?)x.Element(wpNs + "post_name") ?? "",
                        Categories = x.Elements("category")
                                      .Select(c => new WpCategory
                                      {
                                          Domain = (string?)c.Attribute("domain") ?? "",
                                          Nicename = (string?)c.Attribute("nicename") ?? "",
                                          Name = c.Value?.Trim() ?? ""
                                      }).ToList(),
                        YoastTitle = GetMetaValue("_yoast_wpseo_title"),
                        YoastMetaDescription = GetMetaValue("_yoast_wpseo_metadesc")
                    };
                })
                .Where(p => (p.PostType == "post" || p.PostType == "page") && p.Status == "publish") // Filter as needed
                .OrderBy(p => p.Link) // Sort by Link
                .ToList();

            return posts;
        }


        private DateTime ParseDate(string dateStr)
        {
            return DateTime.TryParse(dateStr, out var date) ? date : DateTime.MinValue;
        }
    }
}
