using System.Xml.Linq;

namespace WordPressXmlImport
{
    public class WpCategory
    {
        public required string Domain { get; set; }
        public required string Nicename { get; set; }
        public required string Name { get; set; }
        public string? TermId { get; set; } // optionel for wp:category
    }

    public class WordPressPost
    {
        public required int Id { get; set; }
        public required string Creator { get; set; }
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
        public int? MediaThumbnailId { get; set; }
        public List<(string Title, string Text)> ProjectDetails { get; set; } = new();
        public string SitecoreItemName =>
    Slug.Length > 100
        ? Slug.Substring(0, 100).Replace("%", string.Empty)
        : Slug.Replace("%", string.Empty);
    }

    public class WordPressMedia
    {
        public required int Id { get; set; }
        public required string Url { get; set; }
        public required string Title { get; set; }
        public required string PostType { get; set; }
        public required DateTime PostDate { get; set; }
        public required string Status { get; set; }
        public required string Postname { get; set; }
        // Getter for SitecoreItemName
        public string SitecoreItemName =>
    Postname.Length > 100
        ? Postname.Substring(0, 100).Replace("%", string.Empty)
        : Postname.Replace("%", string.Empty);

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

                    // Extract project details
                    var projectDetails = new List<(string Title, string Text)>();
                    for (int i = 1; i <= 6; i++) // max 6 projecttitels en teksten
                    {
                        var title = GetMetaValue($"_projectTitel{i}") ?? string.Empty;
                        var text = GetMetaValue($"_projectTekst{i}") ?? string.Empty;

                        if (!string.IsNullOrWhiteSpace(title) || !string.IsNullOrWhiteSpace(text))
                        {
                            projectDetails.Add((title, text));
                        }
                    }

                    return new WordPressPost
                    {
                        Id = (int?)x.Element(wpNs + "post_id") ?? -1,
                        Creator = (string?)x.Element(dcNs + "creator") ?? "SitecoreCommander",
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
                        YoastMetaDescription = GetMetaValue("_yoast_wpseo_metadesc"),
                        MediaThumbnailId = int.TryParse(GetMetaValue("_thumbnail_id"), out var thumbnailId) ? (int?)thumbnailId : null,
                        ProjectDetails = projectDetails // Add projectdetails
                    };
                })
                .Where(p => (p.PostType == "post" || p.PostType == "page" || p.PostType == "so_cpt_press" || p.PostType == "so_cpt_projects" || p.PostType == "so_cpt_artikel") && p.Status == "publish") // Filter as needed
                .OrderBy(p => p.Link) // Sort by Link
                .ToList();

            return posts;
        }

        public List<WordPressMedia> LoadMedia(string filePath)
        {
            var doc = XDocument.Load(filePath);

            var posts = doc.Descendants("item")
                .Select(x =>
                {
                    var postMetas = x.Elements(wpNs + "postmeta");

                    return new WordPressMedia
                    {
                        Id = (int?)x.Element(wpNs + "post_id") ?? -1,
                        Url = (string?)x.Element(wpNs + "attachment_url") is string link && Uri.TryCreate(link, UriKind.Absolute, out var uri)
                            ? uri.ToString()
                            : "",
                        Title = (string?)x.Element("title") ?? "",
                        PostType = (string?)x.Element(wpNs + "post_type") ?? "",
                        PostDate = ParseDate((string?)x.Element(wpNs + "post_date") ?? ""),
                        Status = (string?)x.Element(wpNs + "status") ?? "",
                        Postname = (string?)x.Element(wpNs + "post_name") ?? "",
                    };
                })
                .Where(p => (p.PostType == "attachment" && (p.Status == "publish" || p.Status == "inherit"))) // Filter as needed
                .ToList();

            return posts;
        }

        public List<WpCategory> LoadCategory(string filePath)
        {
            var doc = XDocument.Load(filePath);
            var wpNs = "http://wordpress.org/export/1.2/";

            var categories = doc.Descendants(wpNs + "category")
                .Select(x => new WpCategory
                {
                    Domain = "category", // default WordPress domain
                    TermId = (string?)x.Element(wpNs + "term_id"),
                    Nicename = (string?)x.Element(wpNs + "category_nicename") ?? "",
                    Name = (string?)x.Element(wpNs + "cat_name") ?? ""
                })
                .ToList();

            return categories;
        }

        private DateTime ParseDate(string dateStr)
        {
            return DateTime.TryParse(dateStr, out var date) ? date : DateTime.MinValue;
        }
    }
}
