using HtmlAgilityPack;
using RaiXpToCloudMigrator.XmCloud;
using SitecoreCommander.Authoring;
using SitecoreCommander.Authoring.Model;
using SitecoreCommander.Edge.Model;
using SitecoreCommander.WordPress.Model;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using WordPressXmlImport;
using static System.Net.Mime.MediaTypeNames;

namespace SitecoreCommander.WordPress
{
    class WordPressSampleImport
    {
        public static string? GetParentPath(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return null;
            path = path.Trim();
            path = path.Replace('\\', '/');
            if (path.Length > 1 && path.EndsWith("/"))
                path = path.TrimEnd('/');

            var parent = Path.GetDirectoryName(path.Replace('/', Path.DirectorySeparatorChar));
            parent = parent?.Replace('\\', '/');

            return string.IsNullOrEmpty(parent) || parent == "." ? "/" : parent;
        }

        private List<WordPressBlock> OptimizeBlocks(List<WordPressBlock> blocks)
        {
            var optimized = new List<WordPressBlock>();

            foreach (var block in blocks)
            {
                // Detect a YouTube embed in a <figure> block
                if (block.BlockType == "figure")
                {
                    // Try to find a YouTube URL in the inner HTML
                    var match = Regex.Match(block.InnerHtml, @"https:\/\/www\.youtube\.com\/watch\?v=([A-Za-z0-9_\-]+)", RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        var url = match.Value;
                        // Create a new embed block
                        optimized.Add(new WordPressBlock
                        {
                            BlockType = "embed",
                            AttributesJson = System.Text.Json.JsonSerializer.Serialize(new Dictionary<string, object>
                    {
                        { "providerNameSlug", "youtube" },
                        { "url", url }
                    }),
                            InnerHtml = block.InnerHtml,
                            Column = block.Column,
                            TotalColumns = block.TotalColumns
                        });
                        continue;
                    }
                }
                // Default: keep the block as is
                optimized.Add(block);
            }

            return optimized;
        }

        public List<WordPressBlock> ParseBlocks(string content)
        {
            var blocks = new List<WordPressBlock>();

            var pattern = @"<!--\s*wp:([^\s{]+)(\s*\{[^}]*\})?\s*-->(.*?)<!--\s*/wp:\1\s*-->";
            var regex = new Regex(pattern, RegexOptions.Singleline);

            var matches = regex.Matches(content);
            foreach (Match match in matches)
            {
                var blockType = match.Groups[1].Value.Trim();
                var attributes = match.Groups[2].Value.Trim();
                var innerHtml = match.Groups[3].Value.Trim();

                // Special handling for wp:sogutenberg/smallecontent
                if (blockType == "sogutenberg/smallecontent")
                {
                    var parsedBlocks = HandleSmalleContentBlock(attributes, innerHtml);
                    if (parsedBlocks != null && parsedBlocks.Count > 0)
                    {
                        blocks.AddRange(parsedBlocks);
                        continue;
                    }
                }

                // Special handling for wp:columns
                if (blockType == "columns")
                {
                    var parsedBlocks = HandleColumnsBlock(attributes, innerHtml);
                    if (parsedBlocks != null && parsedBlocks.Count > 0)
                    {
                        blocks.AddRange(parsedBlocks);
                    }
                    continue;
                }

                blocks.Add(new WordPressBlock
                {
                    BlockType = blockType,
                    AttributesJson = string.IsNullOrEmpty(attributes) ? null : attributes,
                    InnerHtml = innerHtml
                });
            }
            if (blocks.Count == 0 && !string.IsNullOrWhiteSpace(content))
            {
                // Fallback: avoid infinite loops
                blocks.Add(new WordPressBlock
                {
                    BlockType = "raw",
                    AttributesJson = null,
                    InnerHtml = content
                });
            }
            blocks = OptimizeBlocks(blocks);
            return blocks;
        }

        private List<WordPressBlock> HandleColumnsBlock(string attributes, string innerHtml)
        {
            var blocks = new List<WordPressBlock>();

            // Remove only wp:column comments
            innerHtml = Regex.Replace(innerHtml, @"<!--\s*wp:column\s*-->|<!--\s*/wp:column\s*-->", string.Empty, RegexOptions.Singleline);

            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(innerHtml);

            // Select all columns
            var columnNodes = doc.DocumentNode.SelectNodes("//div[contains(concat(' ', normalize-space(@class), ' '), ' wp-block-column ')]")
                  ?? new HtmlNodeCollection(null);

            if (columnNodes != null)
            {
                int totalColumns = columnNodes.Count;
                int columnIndex = 0;

                foreach (var columnNode in columnNodes)
                {
                    columnIndex++;

                    var columnContent = columnNode.InnerHtml;
                    var regex = new Regex(@"<!--\s*wp:([^\s{]+)(\s*\{[^}]*\})?\s*-->(.*?)<!--\s*/wp:\1\s*-->", RegexOptions.Singleline);
                    var matches = regex.Matches(columnContent);

                    if (matches.Count > 0)
                    {
                        foreach (Match match in matches)
                        {
                            var innerBlocks = ParseBlocks(match.Value);
                            foreach (var innerBlock in innerBlocks)
                            {
                                innerBlock.Column = columnIndex;
                                innerBlock.TotalColumns = totalColumns;
                                blocks.Add(innerBlock);
                            }
                        }
                    }
                    else if (!string.IsNullOrWhiteSpace(columnContent))
                    {
                        // No matches: fallback (HTML)
                        var innerBlocks = ParseBlocks(columnContent);
                        foreach (var innerBlock in innerBlocks)
                        {
                            innerBlock.Column = columnIndex;
                            innerBlock.TotalColumns = totalColumns;
                            blocks.Add(innerBlock);
                        }
                    }
                }
            }

            return blocks;
        }

        private List<WordPressBlock> HandleSmalleContentBlock(string attributes, string innerHtml)
        {
            var blocks = new List<WordPressBlock>();
            var doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(innerHtml);

            var rootNode = doc.DocumentNode;
            if (rootNode.FirstChild?.Name == "div" &&
                rootNode.FirstChild.GetAttributeValue("class", "").Contains("wp-block-sogutenberg-smallecontent"))
            {
                rootNode = rootNode.FirstChild;
            }

            // Check if the block contains only one child node
            var childNodes = rootNode.SelectNodes("./*");
            if (childNodes != null && childNodes.Count == 1)
            {
                var singleChild = childNodes[0];

                // Check if the single child is a supported block type
                if (singleChild.Name == "div" && singleChild.GetAttributeValue("class", "").Contains("wp-block-sogutenberg-quote"))
                {
                    // Convert to wp:sogutenberg/quote
                    blocks.Add(new WordPressBlock
                    {
                        BlockType = "sogutenberg/quote",
                        AttributesJson = attributes,
                        InnerHtml = singleChild.OuterHtml
                    });
                    return blocks;
                }
                else if (singleChild.Name == "figure" && singleChild.GetAttributeValue("class", "").Contains("wp-block-image"))
                {
                    // Convert to wp:image
                    blocks.Add(new WordPressBlock
                    {
                        BlockType = "image",
                        AttributesJson = attributes,
                        InnerHtml = singleChild.OuterHtml
                    });
                    return blocks;
                }
            }

            // If the block contains multiple child nodes, split them into separate blocks
            if (childNodes != null && childNodes.Count > 1)
            {
                foreach (var child in childNodes)
                {
                    if (child.NodeType == HtmlAgilityPack.HtmlNodeType.Element)
                    {
                        blocks.Add(new WordPressBlock
                        {
                            BlockType = child.Name.ToLower(),
                            AttributesJson = attributes,
                            InnerHtml = child.OuterHtml
                        });
                    }
                }
                return blocks;
            }

            // If the block contains multiple child nodes, merge supported tags into one paragraph block
            if (childNodes != null)
            {
                var supportedTags = new HashSet<string> { "h1", "h2", "h3", "h4", "h5", "h6", "p", "ul", "div", "blockquote", "a", "i", "figure", "img" };
                var mergedContent = new StringBuilder();

                foreach (var child in childNodes)
                {
                    if (child.NodeType == HtmlAgilityPack.HtmlNodeType.Element && supportedTags.Contains(child.Name.ToLower()))
                    {
                        // Append the content of supported tags
                        mergedContent.Append(child.OuterHtml);
                    }
                    else
                    {
                        // If unsupported or a break in sequence, finalize the current paragraph block
                        if (mergedContent.Length > 0)
                        {
                            Console.WriteLine("Adding merged content as a paragraph block.");
                            blocks.Add(new WordPressBlock
                            {
                                BlockType = "paragraph",
                                AttributesJson = attributes,
                                InnerHtml = mergedContent.ToString()
                            });
                            mergedContent.Clear();
                        }

                        // Add unsupported elements as separate blocks
                        if (child.NodeType == HtmlAgilityPack.HtmlNodeType.Element)
                        {
                            Console.WriteLine("Adding unsupported content ."+ child.Name.ToLower());
                            blocks.Add(new WordPressBlock
                            {
                                BlockType = child.Name.ToLower(),
                                AttributesJson = attributes,
                                InnerHtml = child.OuterHtml
                            });
                        }
                    }
                }

                // Add any remaining merged content as a paragraph block
                if (mergedContent.Length > 0)
                {
                    blocks.Add(new WordPressBlock
                    {
                        BlockType = "paragraph",
                        AttributesJson = attributes,
                        InnerHtml = mergedContent.ToString()
                    });
                }
            }

            return blocks;
        } 


        public static string MapDomainString(string input)
        {
            var mapping = new Dictionary<string, string>
            {
                { "artikel_categories", "PublicationCategories" },
                { "categorie", "PublicationCategories" },
                { "press_categories", "PublicationCategories" },
                { "press_places", "Locations" }
            };

            return mapping.TryGetValue(input, out var result) ? result : input;
        }

        private string? GetOrCreatListTag(string domain, string tag, string tagFolder, string language, EnvironmentConfiguration env)
        {
            string newTagfolder = tagFolder.Replace("/Data/Tags","/Data/Lists");
            string newDomain = MapDomainString(domain);
            return GetOrCreateTag(newDomain, tag, newTagfolder, language, env);
        }

        private Dictionary<string, string> sitecoretagids = new Dictionary<string, string>();
        private Dictionary<string, string> sitecoretagfolders = new Dictionary<string, string>();
        //Dont make async, the create process should be handled one by one,
        private string? GetOrCreateTag(string domain, string tag, string tagFolder, string language, EnvironmentConfiguration env)
        {
            if (string.IsNullOrWhiteSpace(tag))
                return null;

            var tagSitecoreId = sitecoretagids.TryGetValue(domain + "/" + tag, out string? resolvedId) ? resolvedId : null;
            if (tagSitecoreId != null)
            {
                return tagSitecoreId;
            }

            var tagItemName = Helper.ToValidItemName(tag);
            var tagItem = GetItemSecurity.Get(env, CancellationToken.None, tagFolder.TrimEnd('/') + "/" + domain + "/" + tagItemName)
                                          .GetAwaiter().GetResult();

            if (tagItem != null)
            {
                Console.WriteLine("Tag already exists: " + tagItemName);
                sitecoretagids.Add(domain + "/" + tag, tagItem.itemIdEnclosedInBraces);
                return tagItem.itemIdEnclosedInBraces;
            }
            else
            {
                var tagMainFolderId = sitecoretagfolders.TryGetValue(tagFolder, out string? resolvedRootTagFolderItemId) ? resolvedRootTagFolderItemId : null;
                if (tagMainFolderId == null)
                {
                    var tagMainFolder = GetItemSecurity.Get(env, CancellationToken.None, tagFolder.TrimEnd('/'))
                                                       .GetAwaiter().GetResult();
                    if (tagMainFolder == null)
                    {
                        throw new Exception($"Tag folder {tagFolder} not found");
                    }
                    tagMainFolderId = tagMainFolder.itemIdEnclosedInBraces;
                    sitecoretagfolders.Add(tagFolder, tagMainFolderId);
                }

                var tagItemFolderId = sitecoretagfolders.TryGetValue(tagFolder + "/" + domain, out string? resolvedParentItemId) ? resolvedParentItemId : null;
                if (tagItemFolderId == null)
                {
                    var tagItemFolder = GetItemSecurity.Get(env, CancellationToken.None, tagFolder.TrimEnd('/') + "/" + domain)
                                                       .GetAwaiter().GetResult();
                    if (tagItemFolder == null)
                    {
                        var tagFolderTemplateID = CreateFolderItem.TagFolderID;
                        if (tagFolder.Contains("/Lists"))
                        {
                            tagFolderTemplateID = Templates.CustomListFolderTemplateGuid;
                        }
                        var createdTagFolder = CreateFolderItem.CreateMap(env, CancellationToken.None, domain, tagFolderTemplateID, tagMainFolderId, language, [])
                                                               .GetAwaiter().GetResult();
                        tagItemFolderId = createdTagFolder.itemIdEnclosedInBraces;
                    } else {
                        tagItemFolderId = tagItemFolder.itemIdEnclosedInBraces;
                    }
                    sitecoretagfolders.Add(tagFolder + "/" + domain, tagItemFolderId);
                }

                var createdTag = new Created();
                if (tagFolder.Contains("/Lists"))
                {
                    var fieldNameValues = new Dictionary<string, string>
                    {
                        { "Title", tag }
                    };
                    createdTag = AddItem.Create(env, CancellationToken.None, Helper.ToValidItemName(tag), Templates.CustomListTagItemTemplateGuid, tagItemFolderId, language, fieldNameValues)
                                        .GetAwaiter().GetResult();
                }
                else
                {
                    createdTag = CreateTagItem.CreateTag(env, CancellationToken.None, tag, tagItemFolderId, language)
                                              .GetAwaiter().GetResult();
                }
                sitecoretagids.Add(domain + "/" + tag, createdTag.itemIdEnclosedInBraces);
                return createdTag.itemIdEnclosedInBraces;
            }
        }

        //Dont make async, the create process should be handled one by one,
        private Dictionary<int, string> wpsitecoremediaid = new Dictionary<int, string>();
        private Dictionary<string, string> sitecoremediapathid = new Dictionary<string, string>();
        private string? GetOrCreateMedia(WordPressMedia wpmedia, string foldername, string subfolder, string mediaFolder, string language, EnvironmentConfiguration env)
        {
            if (wpmedia == null)
                return null;
            var mediaSitecoreId = wpsitecoremediaid.TryGetValue(wpmedia.Id, out string? resolvedId) ? resolvedId : null;
            if (mediaSitecoreId != null)
            {
                return mediaSitecoreId;
            }
            var mediaMainFolderId = sitecoremediapathid.TryGetValue(mediaFolder, out string? resolvedRootMediaFolderItemId) ? resolvedRootMediaFolderItemId : null;
            if (resolvedRootMediaFolderItemId == null)
            {
                var mediaMainFolder = GetItemSecurity.Get(env, CancellationToken.None, mediaFolder.TrimEnd('/'))
                                                   .GetAwaiter().GetResult();
                if (mediaMainFolder == null)
                {
                     throw new Exception($"Media folder {mediaFolder} not found");
                }
                mediaMainFolderId = mediaMainFolder.itemIdEnclosedInBraces;
                sitecoremediapathid.Add(mediaFolder.TrimEnd('/'), mediaMainFolderId);
            }
            var optionalSubfolder = "/";
            if (!string.IsNullOrEmpty(subfolder))
            {
                optionalSubfolder = "/"+ subfolder+"/";
            }
            var mediapath = mediaFolder.TrimEnd('/') + "/" + foldername.TrimEnd('/') +"/" + wpmedia.PostDate.Year.ToString() + optionalSubfolder + wpmedia.SitecoreItemName.TrimEnd('/');
            var mediaId = sitecoremediapathid.TryGetValue(mediapath, out string? resolvedMediaItemId) ? resolvedMediaItemId : null;
            if (mediaId != null)
            {
                var parsedMediaId = Guid.Parse(mediaId).ToString("B").ToUpper();
                wpsitecoremediaid.Add(wpmedia.Id, parsedMediaId);
                return parsedMediaId;
            }
            var media = GetItemSecurity.Get(env, CancellationToken.None, mediapath.TrimEnd('/'))
                                                .GetAwaiter().GetResult();
            if (media != null)
            {
                mediaId = media.itemIdEnclosedInBraces;
                sitecoremediapathid.Add(mediapath.TrimEnd('/'), mediaId);
                var parsedMediaId = Guid.Parse(mediaId).ToString("B").ToUpper();
                wpsitecoremediaid.Add(wpmedia.Id, parsedMediaId);
                return mediaId;
            }
            try
            {
                var upload = AddMedia.Create(env, CancellationToken.None, mediapath, language, wpmedia.Title.Replace("\\", ""), wpmedia.Url).GetAwaiter().GetResult();
                sitecoremediapathid.Add(mediapath.TrimEnd('/'), upload.ItemIdEnclosedInBraces);
                return upload.ItemIdEnclosedInBraces;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WARNING] Media upload failed for '{wpmedia.Url}': {ex.Message}");
                return null;
            }

        }
        private string? GetOrCreateMedia(int wpmediaid, string foldername, string subfolder, string mediaFolder, string language, EnvironmentConfiguration env)
        {
            var wpmediaImg = mediaPosts.FirstOrDefault(x => x.Id == wpmediaid);
            if (wpmediaImg == null)
            {
                Console.WriteLine($"WARNING: Media with ID {wpmediaid} not found.");
                return null;
            }
            return GetOrCreateMedia(wpmediaImg, foldername, subfolder, mediaFolder, language, env);
        }

        private string CreateFinalLayout(string posttype, string content, List<WordPressBlock> blocks)
        {
            string final = "<r xmlns:p=\"p\" xmlns:s=\"s\" p:p=\"1\"><d id=\"{FE5D7FDF-89C0-4D99-9AA3-B5FBD009C9F3}\">";
            string placeholder = "/headless-main/column-1-2";
            if (posttype == "page")
            {
                //add eveything you need on every page here
            }
            if (posttype == "so_cpt_artikel")
            {
                placeholder = "/headless-main/sxa-articleheader/publicationheader-1";
            }
            else if (posttype == "so_cpt_press")
            {
                placeholder = "/headless-main/sxa-pressreleaseheader/publicationheader-1";
            }
            else if (posttype == "post")
            {
                placeholder = "/headless-main/sxa-newsheader/publicationheader-1";
            }
            int count = 0;
            if (!string.IsNullOrWhiteSpace(content))
            {
                //add the current page as text component
                final += "<r uid=\"" + Guid.NewGuid().ToString("B").ToUpper() + "\" s:ds=\"page:\" s:id=\""+ Templates.TextComponentRenderingGuid + "\" s:par=\"FieldNames=%7BFAEF6224-3F5E-45FC-A20C-E2CA3174CD8E%7D&amp;Styles&amp;RenderingIdentifier&amp;CSSStyles&amp;DynamicPlaceholderId=" + count + "\" s:ph=\"/headless-main/sxa-banner/column-1-2\" />";
            }
            count = 1;
            foreach (var block in blocks)
            {
                count++;
                switch (block.BlockType.ToLower())
                {
                    case "h2":
                    case "h3":
                    case "heading":
                        Console.WriteLine("Handling a heading, title block.");
                        final += "<r uid=\"" + Guid.NewGuid().ToString("B").ToUpper() + "\" s:ds=\"local:/Data/Title" + count + "\" s:id=\""+ Templates.HeadingComponentRenderingGuid + "\" s:par=\"FieldNames=%7B193F7152-1E45-41BE-BE9C-E0D186307F49%7D&amp;Styles=%7BD26E71C2-937B-422C-85C6-DC3168D3DF38%7D&amp;RenderingIdentifier&amp;CSSStyles&amp;DynamicPlaceholderId=" + count + "\" s:ph=\""+ placeholder+"\" />";
                        break;
                    case "p":
                    case "div":
                    case "list":
                    case "paragraph":
                        Console.WriteLine("Handling a paragraph block.");
                        final += "<r uid=\"" + Guid.NewGuid().ToString("B").ToUpper() + "\" s:ds=\"local:/Data/Text" + count + "\" s:id=\""+ Templates.TextComponentRenderingGuid + "\" s:par=\"FieldNames=%7BFAEF6224-3F5E-45FC-A20C-E2CA3174CD8E%7D&amp;Styles&amp;RenderingIdentifier&amp;CSSStyles&amp;DynamicPlaceholderId="+count+ "\" s:ph=\"" + placeholder + "\" />";
                        break;
                    case "sogutenberg/smallecontent":
                        Console.WriteLine("Handling a sogutenberg/smallecontent block.");
                        final += "<r uid=\"" + Guid.NewGuid().ToString("B").ToUpper() + "\" s:ds=\"local:/Data/RichText" + count + "\" s:id=\""+ Templates.TextComponentRenderingGuid + "\" s:par=\"FieldNames=%7BFAEF6224-3F5E-45FC-A20C-E2CA3174CD8E%7D&amp;Styles&amp;RenderingIdentifier&amp;CSSStyles&amp;DynamicPlaceholderId=" + count + "\" s:ph=\"" + placeholder + "\" />";
                        break;

                    case "media-text":
                        Console.WriteLine("Handling an media-text block.");
                        final += "<r uid=\"" + Guid.NewGuid().ToString("B").ToUpper() + "\" s:ds=\"local:/Data/MediaText" + count + "\" s:id=\""+ Templates.TextComponentRenderingGuid + "\" s:par=\"FieldNames=%7BFAEF6224-3F5E-45FC-A20C-E2CA3174CD8E%7D&amp;Styles&amp;RenderingIdentifier&amp;CSSStyles&amp;DynamicPlaceholderId=" + count + "\" s:ph=\"" + placeholder + "\" />";
                        break;

                    case "sogutenberg/quote":
                        Console.WriteLine("Handling an quote block.");
                        final += "<r uid=\"" + Guid.NewGuid().ToString("B").ToUpper() + "\" s:ds=\"local:/Data/Quote" + count + "\" s:id=\"" + Templates.QuoteComponentRenderingGuid + "\" s:par=\"FieldNames=%7BF1D8A6AB-FD85-4343-BDED-10DD304B3B6A%7D&amp;Styles&amp;RenderingIdentifier&amp;CSSStyles&amp;DynamicPlaceholderId=" + count + "\" s:ph=\"" + placeholder + "\" />";
                        break;
                    case "sogutenberg/cta":
                        Console.WriteLine("Handling a cta block.");

                        // Add the main CTA block to final
                        final += "<r uid=\"" + Guid.NewGuid().ToString("B").ToUpper() + "\" s:ds=\"local:/Data/CTA" + count + "\" s:id=\"" + Templates.CTAComponentRenderingGuid + "\" s:par=\"FieldNames=%7BF1D8A6AB-FD85-4343-BDED-10DD304B3B6A%7D&amp;Styles&amp;RenderingIdentifier&amp;CSSStyles&amp;DynamicPlaceholderId=" + count + "\" s:ph=\"" + placeholder + "\" />";

                        // Parse the inner HTML to extract buttons
                        var ctaDoc = new HtmlAgilityPack.HtmlDocument();
                        ctaDoc.LoadHtml(block.InnerHtml);

                        // Select all buttons within the cta block
                        var buttonNodes = ctaDoc.DocumentNode.SelectNodes("//div[contains(@class, 'wp-block-button')]");
                        if (buttonNodes != null && buttonNodes.Count > 0)
                        {
                            // Loop through buttons and add extra strings for buttons beyond the first
                            for (int i = 1; i < buttonNodes.Count; i++) // Start from the second button
                            {
                                var buttonNode = buttonNodes[i];
                                var buttonLink = buttonNode.SelectSingleNode(".//a")?.GetAttributeValue("href", string.Empty) ?? string.Empty;
                                var buttonText = buttonNode.SelectSingleNode(".//a")?.InnerText ?? string.Empty;

                                // Add extra string for this button
                                final += "<r uid=\"" + Guid.NewGuid().ToString("B").ToUpper() + "\" s:ds=\"local:/Data/CTAButton" + count + "-" + i + "\" s:id=\"" + Templates.CTAButtonComponentRenderingGuid + "\" s:par=\"FieldNames=%7BF1D8A6AB-FD85-4343-BDED-10DD304B3B6A%7D&amp;Styles&amp;RenderingIdentifier&amp;CSSStyles&amp;DynamicPlaceholderId=" + count + "0" + i + "\" s:ph=\"" + placeholder + "\" />";
                            }
                        }
                        else
                        {
                            Console.WriteLine("No buttons found in the cta block.");
                        }
                        break;
                    case "figure":
                    case "image":
                        Console.WriteLine("Handling a image block.");
                        final += "<r uid=\"" + Guid.NewGuid().ToString("B").ToUpper() + "\" s:ds=\"local:/Data/Image" + count + "\" s:id=\"" + Templates.ImageComponentRenderingGuid + "\" s:par=\"FieldNames=%7BF1D8A6AB-FD85-4343-BDED-10DD304B3B6A%7D&amp;Styles&amp;RenderingIdentifier&amp;CSSStyles&amp;DynamicPlaceholderId=" + count + "\" s:ph=\"" + placeholder + "\" />";
                        break;

                    case "gallery":
                        Console.WriteLine("Handling a gallery block.");
                        final += "<r uid=\"" + Guid.NewGuid().ToString("B").ToUpper() + "\" s:ds=\"local:/Data/Gallery" + count + "\" s:id=\"" + Templates.GalleryComponentRenderingGuid + "\" s:par=\"FieldNames=%7BBE2CB9F9-69C1-45BA-8B1C-ACED2345E648%7D&amp;Styles&amp;RenderingIdentifier&amp;CSSStyles&amp;DynamicPlaceholderId=" + count + "\" s:ph=\"" + placeholder + "\" />";
                        break;
                    case "button":
                    case "buttons":
                        Console.WriteLine("Handling a button block.");
                        final += "<r uid=\"" + Guid.NewGuid().ToString("B").ToUpper() + "\" s:ds=\"local:/Data/Button" + count + "\" s:id=\"" + Templates.CTAButtonComponentRenderingGuid + "\" s:par=\"FieldNames=%7BBE2CB9F9-69C1-45BA-8B1C-ACED2345E648%7D&amp;Styles&amp;RenderingIdentifier&amp;CSSStyles&amp;DynamicPlaceholderId=" + count + "\" s:ph=\"" + placeholder + "\" />";
                        break;
                    case "embed":
                        Console.WriteLine("Handling a embed block.");
                        var attributes = block.AttributesJson ?? "{}";
                        var parsedAttributes = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(attributes);
                        var providername = parsedAttributes?.GetValueOrDefault("providerNameSlug")?.ToString();
                        if (providername == "youtube")
                        {
                            final += "<r uid=\"" + Guid.NewGuid().ToString("B").ToUpper() + "\" s:ds=\"local:/Data/YouTube" + count + "\" s:id=\"" + Templates.YouTubeComponentRenderingGuid + "\" s:par=\"FieldNames=%7BBE2CB9F9-69C1-45BA-8B1C-ACED2345E648%7D&amp;Styles&amp;RenderingIdentifier&amp;CSSStyles&amp;DynamicPlaceholderId=" + count + "\" s:ph=\"" + placeholder + "\" />";
                        } else
                        {
                            Console.WriteLine($"Unknown embed block type: {providername}");
                        }
                            break;
                    case "spacer":
                        Console.WriteLine("Ignore spacer");
                        break;
                    case "separator":
                        Console.WriteLine("Ignore spacer");
                        break;
                    case "html":
                        Console.WriteLine("Ignore html block");
                        break;
                    default:
                        Console.WriteLine($"Unknown block type: {block.BlockType}");
                        break;
                }
            }

            final += "</d></r>";
            return final;
        }
        private bool CreateLocalDataSources(EnvironmentConfiguration env, List<WordPressBlock> blocks, string ItemGuid, string language, string mediaFolderPath, string name)
        {
            var datafolder = CreateFolderItem.CreateHiddenDataFolder(env, CancellationToken.None, "Data", ItemGuid, language).GetAwaiter().GetResult();
            int count = 1;

            foreach (var block in blocks)
            {
                count++;
                switch (block.BlockType.ToLower())
                {
                    case "h2":
                    case "h3":
                    case "heading":
                        Console.WriteLine("Handling a heading, title block");
                        var fieldNameValuesHeading = new Dictionary<string, string>
                        {
                            //{ "Title", Helper.StripHtmlTags(block.InnerHtml) },
                            { "Text", Helper.StripHtmlTags(block.InnerHtml) },
                        };
                        var updated = AddItem.Create(env, CancellationToken.None, "Title" + count, Templates.HeadingComponentTemplateGuid, datafolder.itemIdEnclosedInBraces, language, fieldNameValuesHeading);
                        break;
                    case "p":
                    case "div":
                    case "list":
                    case "ul":
                    case "paragraph":
                        Console.WriteLine("Handling a paragraph block.");
                        var fieldNameValuesText = new Dictionary<string, string>
                        {
                            { "Text", block.InnerHtml },
                        };
                        var updated2 = AddItem.Create(env, CancellationToken.None, "Text" + count, Templates.TextComponentTemplateGuid, datafolder.itemIdEnclosedInBraces, language, fieldNameValuesText);
                        break;
                    case "sogutenberg/smallecontent":
                        Console.WriteLine("Handling a sogutenberg/smallecontent block.");
                        var renderer = new SmalleContentRenderer
                        {
                            GetOrCreateMediaDelegate = GetOrCreateMedia,
                            mediaFolder = mediaFolderPath,
                            language = language,
                            env = env
                        };
                        var textcontent = renderer.Render(block);
                        var fieldNameValuesText3 = new Dictionary<string, string>
                        {
                            { "Text", Helper.StripHtmlTags(textcontent) },
                        };
                        var updated3 = AddItem.Create(env, CancellationToken.None, "RichText" + count, Templates.TextComponentTemplateGuid, datafolder.itemIdEnclosedInBraces, language, fieldNameValuesText3);
                        break;

                    case "media-text":
                        Console.WriteLine("Handling an media-text block.");
                        var mtrenderer = new WordPressMediaTextRenderer
                        {
                            GetOrCreateMediaDelegate = GetOrCreateMedia,
                            mediaFolder = mediaFolderPath,
                            language = language,
                            env = env
                        };
                        var mediarichText = mtrenderer.Render(block);
                        var fieldNameValuesMediaText3 = new Dictionary<string, string>
                        {
                            { "Text", Helper.StripHtmlTags(mediarichText) },
                        };
                        var updatedMediaText = AddItem.Create(env, CancellationToken.None, "MediaText" + count, Templates.TextComponentTemplateGuid, datafolder.itemIdEnclosedInBraces, language, fieldNameValuesMediaText3);
                        break;
                    case "sogutenberg/quote":
                        Console.WriteLine("Handling a quote block.");
                        string mediastring = string.Empty;
                        var attributes = block.AttributesJson ?? "{}";
                        var parsedAttributes = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(attributes);
                        var quoteWpImgid = parsedAttributes?.GetValueOrDefault("imageID1");
                        if (!string.IsNullOrEmpty(quoteWpImgid))
                        {
                            int intmediaid = int.Parse(quoteWpImgid);
                            var quouteSitecoreMediaId = GetOrCreateMedia(intmediaid, "Quote", "", mediaFolderPath, language, env);
                            if (quouteSitecoreMediaId != null)
                            {
                                mediastring = "<image mediaid=\"" + quouteSitecoreMediaId + "\" />";
                            }
                        }
                        var fieldNameQuote = new Dictionary<string, string>
                        {
                            { "Title", parsedAttributes?.GetValueOrDefault("naam") ?? string.Empty },
                            { "SubTitle", parsedAttributes?.GetValueOrDefault("functie") ?? string.Empty },
                            { "Text", parsedAttributes?.GetValueOrDefault("quote") ?? string.Empty },
                            { "Image", mediastring },
                        };
                        var updatedQuote = AddItem.Create(env, CancellationToken.None, "Quote" + count, Templates.QuoteComponentTemplateGuid, datafolder.itemIdEnclosedInBraces, language, fieldNameQuote);
                        break; // Add this break statement to prevent fall-through
                    case "sogutenberg/cta":
                        Console.WriteLine("Handling a sogutenberg/cta block.");

                        // Parse the inner HTML to extract buttons and heading
                        var ctaDoc = new HtmlAgilityPack.HtmlDocument();
                        ctaDoc.LoadHtml(block.InnerHtml);
                        // Select all buttons within the cta block
                        var buttonNodes = ctaDoc.DocumentNode.SelectNodes("//div[contains(@class, 'wp-block-button')]");
                        // Extract the heading (if available)
                        var headingNode = ctaDoc.DocumentNode.SelectSingleNode("//h2");
                        var headingText = headingNode?.InnerText ?? string.Empty;
                        var firstbuttonlink = "";
                        if (buttonNodes != null && buttonNodes.Count > 0)
                        {
                            firstbuttonlink = buttonNodes[0].SelectSingleNode(".//a")?.GetAttributeValue("href", string.Empty) ?? string.Empty;
                        }
                        // Create the main CTA data source
                        var fieldNameValuesCTA = new Dictionary<string, string>
                        {
                            { "Title", headingText },
                            { "Link",firstbuttonlink }
                        };

                        var updatedCTA = AddItem.Create(env, CancellationToken.None, "CTA" + count, Templates.CTAComponentTemplateGuid, datafolder.itemIdEnclosedInBraces, language, fieldNameValuesCTA);
                        if (updatedCTA != null)
                        {
                            Console.WriteLine($"Created CTA data source: {updatedCTA.Result.itemIdEnclosedInBraces}");
                        }

                        if (buttonNodes != null && buttonNodes.Count > 0)
                        {
                            // Loop through buttons and create data sources for each button, not the first
                            for (int i = 1; i < buttonNodes.Count; i++)
                            {
                                var buttonNode = buttonNodes[i];
                                var buttonLink = buttonNode.SelectSingleNode(".//a")?.GetAttributeValue("href", string.Empty) ?? string.Empty;
                                var buttonText = buttonNode.SelectSingleNode(".//a")?.InnerText ?? string.Empty;
                                // Create a data source for each button
                                var fieldNameValuesButton = new Dictionary<string, string>
                                {
                                    { "Link", "<link text=\""+buttonText+"\" linktype=\"external\" url=\""+buttonLink+"\" anchor=\"\" target=\"\" />" }
                                };

                                var updatedButton = AddItem.Create(env, CancellationToken.None, "CTAButton" + count + "-" + i, Templates.CTAButtonComponentTemplateGuid, datafolder.itemIdEnclosedInBraces, language, fieldNameValuesButton);
                                if (updatedButton != null)
                                {
                                    Console.WriteLine($"Created CTA Button data source: {updatedButton.Result.itemIdEnclosedInBraces}");
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("No buttons found in the sogutenberg/cta block.");
                        }
                        break;
                    case "figure":
                        // let op figure can contain youtube!
                    case "image":
                        Console.WriteLine("Handling an image block.");
                        if (block.BlockType.ToLower() == "figure")
                        {
                            Console.WriteLine("figure -> image");
                        }
                        // Parse the inner HTML to extract the image details
                        var imageDoc = new HtmlAgilityPack.HtmlDocument();
                        imageDoc.LoadHtml(block.InnerHtml);

                        var imgNode = imageDoc.DocumentNode.SelectSingleNode("//img");
                        if (imgNode != null)
                        {
                            var src = imgNode.GetAttributeValue("src", string.Empty);
                            var alt = imgNode.GetAttributeValue("alt", string.Empty);

                            // Extract the WordPress media ID from the class attribute (if available)
                            var classAttr = imgNode.GetAttributeValue("class", string.Empty);
                            var wpMediaIdMatch = Regex.Match(classAttr, @"wp-image-(\d+)");
                            string? sitecoreMediaId = null;

                            if (wpMediaIdMatch.Success)
                            {
                                var wpMediaId = int.Parse(wpMediaIdMatch.Groups[1].Value);
                                sitecoreMediaId = GetOrCreateMedia(wpMediaId, "Images", "", mediaFolderPath, language, env);
                            }

                            if (!string.IsNullOrEmpty(sitecoreMediaId))
                            {
                                // Create the data source for the image block
                                var fieldNameValuesImage = new Dictionary<string, string>
                                {
                                    { "Image", $"<image mediaid=\"{sitecoreMediaId}\" alt=\"{alt}\" />" },
                                    { "ImageCaption", alt }
                                };

                                var updatedImage = AddItem.Create(env, CancellationToken.None, "Image" + count, Templates.ImageComponentTemplateGuid, datafolder.itemIdEnclosedInBraces, language, fieldNameValuesImage);
                                if (updatedImage != null)
                                {
                                    Console.WriteLine($"Created image data source: {updatedImage.Result.itemIdEnclosedInBraces}");
                                }
                            }
                            else
                            {
                                Console.WriteLine("Failed to resolve Sitecore media ID for the image.");
                            }
                        }
                        else
                        {
                            //a figure can also contain a vimeo
                            Console.WriteLine("No valid <img> tag found in the image block.");
                        }
                        break;

                    case "gallery":
                        Console.WriteLine("Handling a gallery block.");
                        var gallertyrenderer = new WordPressGalleryRenderer();
                        var gallery = gallertyrenderer.Render(block);
                        var fieldgallery = new Dictionary<string, string>
                        {
                            { "Title", "" },
                        };
                        var galleryupdated = AddItem.Create(env, CancellationToken.None, "Gallery" + count, Templates.GalleryComponentTemplateGuid, datafolder.itemIdEnclosedInBraces, language, fieldgallery).GetAwaiter().GetResult();
                        var imagecount = 0;
                        foreach(var image  in gallery.Images)
                        {
                            imagecount++;
                            var mediaId = GetOrCreateMedia(image.MediaId, "Gallery", name, mediaFolderPath, language, env);  
                            if (mediaId != null)
                            {
                                var fieldNameValuesImage = new Dictionary<string, string>
                                {
                                    { "Image", "<image mediaid=\"" + mediaId + "\" />" },
                                    { "ImageCaption", image.Alt },
                                };
                                var updated4 = AddItem.Create(env, CancellationToken.None, "Image" + imagecount, Templates.ImageComponentTemplateGuid, galleryupdated.itemIdEnclosedInBraces, language, fieldNameValuesImage);
                            }
                        }
                        break;
                    case "button":
                    case "buttons":
                        Console.WriteLine("Handling a button block.");
                        // Parse the inner HTML of the button block
                        var buttonDoc = new HtmlAgilityPack.HtmlDocument();
                        buttonDoc.LoadHtml(block.InnerHtml);

                        // Select the first <a> tag
                        var anchorNode = buttonDoc.DocumentNode.SelectSingleNode("//a");
                        var singlebuttonLink = "";
                        var singlebuttonText = "";
                        if (anchorNode != null)
                        {
                            // Extract the href attribute and inner text
                            singlebuttonLink = anchorNode.GetAttributeValue("href", string.Empty);
                            singlebuttonText = anchorNode.InnerText.Trim();
                        }
                        // Create a data source for each button
                        var fieldNameValuesSingleButton = new Dictionary<string, string>
                        {
                            { "Link", "<link text=\""+singlebuttonText+"\" linktype=\"external\" url=\""+singlebuttonLink+"\" anchor=\"\" target=\"\" />" }
                        };
                        var updatedlink = AddItem.Create(env, CancellationToken.None, "Button" + count, Templates.CTAButtonComponentTemplateGuid, datafolder.itemIdEnclosedInBraces, language, fieldNameValuesSingleButton);
                        break;
                    case "embed":
                        Console.WriteLine("Handling a embed block.");
                        var embedattributes = block.AttributesJson ?? "{}";
                        var parsedEmbedAttributes = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(embedattributes);
                        var providername = parsedEmbedAttributes?.GetValueOrDefault("providerNameSlug")?.ToString();
                        if (providername == "youtube")
                        {
                            var youtubeurl = parsedEmbedAttributes?.GetValueOrDefault("url")?.ToString();
                            var youtubeid = Helper.GetYouTubeVideoId(youtubeurl);
                            var fieldNameValuesYoutube = new Dictionary<string, string>
                            {
                                { "BaseUrl", "https://www.youtube.com/embed/" },
                                { "VideoId", youtubeid   },
                            };
                            var updatedyoutube = AddItem.Create(env, CancellationToken.None, "YouTube" + count, Templates.YouTubeComponentTemplateGuid, datafolder.itemIdEnclosedInBraces, language, fieldNameValuesYoutube);
                        }
                        else
                        {
                            Console.WriteLine($"Unknown embed block type: {providername}");
                        }
                        break;
                    case "html":
                        Console.WriteLine("ignore Handling a html block.");
                        // Add logic to handle video block  
                        break;
                    case "spacer":
                    case "separator":
                        Console.WriteLine("ignore Handling a separator block.");
                        // Add logic to handle video block  
                        break;
                    case "raw":
                        Console.WriteLine("ignore Handling a raw block.");
                        break;
                    default:
                        Console.WriteLine($"Unknown block type: {block.BlockType}");
                        break;
                }
            }
                return true;
        }

        public static string ReplaceSeoPlaceholders(string input, string? pageTitle = null, string? siteName = null, string? excerpt = null, string? page = null, string? sep = "-")
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            var replacements = new Dictionary<string, string?>
    {
        { "%%title%%", pageTitle },
        { "%%sitename%%", siteName },
        { "%%excerpt%%", excerpt },
        { "%%page%%", page },
        { "%%sep%%", sep }
    };

            foreach (var kvp in replacements)
            {
                if (!string.IsNullOrEmpty(kvp.Value))
                {
                    input = input.Replace(kvp.Key, kvp.Value);
                }
                else
                {
                    input = input.Replace(kvp.Key, string.Empty);
                }
            }

            return input;
        }

        private List<WordPressMedia> mediaPosts = new List<WordPressMedia>();
        public async Task ImportPostsAsync(EnvironmentConfiguration env, ResultGetItem siteroot, string language, string defaulttemplateid, string mediaFolderPath, string tagFolderPath, string filepath, bool overwrite)
        {
            var importer = new ExtendedXmlImporter();
            mediaPosts = importer.LoadMedia(filepath);
            var sortePosts = importer.LoadPosts(filepath);
            var sitecoreids = new Dictionary<string, string>();
            sitecoreids.Add("/", siteroot.itemIdEnclosedInBraces);
            int counter = 0;
            foreach (var post in sortePosts)
            {
                counter++;
                Console.WriteLine($"\n\n[{counter}/{sortePosts.Count}|{post.PostType}] {post.Link} {post.Title} ({post.PostDate}): {post.Slug}");
                bool isNewPressPost = post.PostType == "post" || post.PostType == "so_cpt_press" || post.PostType == "so_cpt_artikel";
                bool isBucketItem = post.PostType == "so_cpt_projects" || isNewPressPost;
                string year = post.PostDate.Year.ToString();
                string checkpath = GetParentPath(siteroot.path + "/Home" + post.Link.TrimEnd('/')) + "/" + post.SitecoreItemName;
                if (isBucketItem)
                {
                    var yearparentPath = GetParentPath("/Home" + post.Link.TrimEnd('/')) + "/" + year + "/" + post.SitecoreItemName;
                    checkpath = siteroot.path + yearparentPath;
                }
                if (post.Link == "/")
                {
                    checkpath = siteroot.path + "/Home";
                }
                var itemTask = GetItemSecurity.Get(env, CancellationToken.None, checkpath);
                var item = await itemTask; // Await the Task to get the actual ResultItemWithSecurity object
                if (overwrite && item != null && post.Link != "/" && post.Link != "/nieuws" && post.Link != "/persberichten/" && post.Link != "/nieuws/")
                {
                     var deleteresult = DeleteItem.Delete(env, CancellationToken.None, item.path).GetAwaiter().GetResult();
                    if (deleteresult)
                    {
                        item = null;
                    }
                }
                if (item != null)
                {
                    // Item exists already.
                    Console.WriteLine("Item already exists: " + item.path);
                    if (!sitecoreids.ContainsKey("/Home" + post.Link.TrimEnd('/')))
                    {
                        sitecoreids.Add("/Home" + post.Link.TrimEnd('/'), item.itemIdEnclosedInBraces);
                    }
                } else
                {
                    var thumbnailImg = mediaPosts.FirstOrDefault(x => x.Id == post.MediaThumbnailId);
                    var thubnaimImgSitecoreId = GetOrCreateMedia(thumbnailImg, "Banners", "", mediaFolderPath, language, env);
                    string tags = string.Empty;
                    var tagList = new List<string?>();
                    var tagListProject_maincategories = new List<string?>();
                    var tagListProject_categories = new List<string?>();
                    var tagListProject_vestigings = new List<string?>(); 
                    var tagListProject_locations = new List<string?>();
                    var tagListProject_woonoplossingen = new List<string?>();
                    var tagListPublicatieCategory = new List<string?>();
                    var tagListPressPlaces = new List<string?>();
                    foreach (var tagitem in post.Categories)
                    {
                       if (tagitem.Domain.StartsWith("press_places"))
                        {
                            tagListPressPlaces.Add(GetOrCreatListTag(tagitem.Domain, tagitem.Name, tagFolderPath, language, env));
                        }
                        else
                            tagList.Add(GetOrCreateTag(tagitem.Domain, tagitem.Name, tagFolderPath, language, env));
                        if (isNewPressPost && tagitem.Domain.Contains("categorie"))
                        {
                            tagListPublicatieCategory.Add(GetOrCreatListTag(tagitem.Domain, tagitem.Name, tagFolderPath, language, env));
                        }
                    }
                    tags = string.Join("|", tagList.Where(tag => !string.IsNullOrEmpty(tag)));
                    var blocks = this.ParseBlocks(post.Content);
                    var content = "";
                    if (blocks != null && blocks.Count > 0)
                    {
                        if (blocks[0].BlockType == "heading")
                        {
                            content = blocks[0].InnerHtml;
                            blocks.RemoveAt(0);
                        }
                        if (blocks[0].BlockType == "paragraph")
                        {
                            content += blocks[0].InnerHtml;
                            blocks.RemoveAt(0);
                        } else if (blocks[0].BlockType == "sogutenberg/smallecontent")
                        {
                            var renderer = new SmalleContentRenderer
                            {
                                GetOrCreateMediaDelegate = GetOrCreateMedia,
                                mediaFolder = mediaFolderPath,
                                language = language, 
                                env = env
                            };
                            content += renderer.Render(blocks[0]);
                            blocks.RemoveAt(0);
                        } else
                        {
                            Console.WriteLine("WARNING: First block is not a paragraph or smallecontent but " + blocks[0].BlockType);
                        }
                    }

                    var yoastTitel = ReplaceSeoPlaceholders(post.YoastTitle, post.Title, "Company name", "");
                    var description = ReplaceSeoPlaceholders(post.YoastMetaDescription, yoastTitel, "Company name", "");
                    
                    var pagetype = "{04EA598F-13D9-450A-8F3B-25D0A8711484}";
                    var templateid = defaulttemplateid;
                    if (post.PostType == "post")
                    {
                        //news page
                        templateid = "{2FC40A7A-ECC6-4A35-990F-1DAECBF791CB}";
                        pagetype = "{BDA43527-452B-40F1-BF7D-5252221B5BDB}";
                    }
                    else if (post.PostType == "page")
                    {
                        //page
                        templateid = Templates.PageTemplateGuid;
                        pagetype = "{04EA598F-13D9-450A-8F3B-25D0A8711484}";
                    }
                    else if (post.PostType == "so_cpt_press")
                    {
                        //press page
                        templateid = "{9B2DD7E9-C15A-4465-822F-38B203978D83}";
                        pagetype = "{D1C8723C-F68E-4305-9EE4-18541EC14690}";
                    }
                    else if (post.PostType == "so_cpt_projects")
                    {
                        //project page
                        templateid = "{3FAAE53A-715B-4871-B951-3778C160F018}";
                        pagetype = "{8C0CC544-F479-4007-8512-BF773A4C69ED}";
                    }
                    else if (post.PostType == "so_cpt_artikel")
                    {
                        //artikel page
                        templateid = "{DB71E15F-152C-4B5A-96AC-057D995087F2}";
                        pagetype = "{338DBD72-E687-4572-BD5C-B90680DFFDDB}";
                    }

                    var title = Helper.StripHtmlTags(post.Title);
                    var fieldNameValues = new Dictionary<string, string>
                    {
                        { "Title", title },
                        { "Content", content },
                        { "__Created", post.PostDate.ToString("yyyyMMdd'T'HHmmss'Z'") },
                        { "SxaTags", tags },
                        //{ "MetaTitle", yoastTitel},
                        { "NavigationTitle", title },
                        //{ "MetaDescription", description},
                        //{ "Image","<image mediaid=\""+thubnaimImgSitecoreId+ "\" />"  },
                        //{ "MetaImage","<image mediaid=\""+thubnaimImgSitecoreId+ "\" />"  },
                        //{ "OpenGraphDescription", description},
                        //{ "OpenGraphTitle", yoastTitel},
                        //{ "PageType", pagetype },
                        //{ "OpenGraphImageUrl","<image mediaid=\""+thubnaimImgSitecoreId+ "\" />"  },
                        { "__Created by", post.Creator },
                        { "__Final Renderings", CreateFinalLayout(post.PostType, content, blocks)  }
                    };

                    if (isNewPressPost)
                    {
                        fieldNameValues.Add("Date", post.PostDate.ToString("yyyyMMdd'T'HHmmss'Z'"));
                        fieldNameValues.Add("Intro", Helper.StripHtmlTags(content));
                        fieldNameValues.Add("Categories", string.Join("|", tagListPublicatieCategory.Where(tag => !string.IsNullOrEmpty(tag))));
                        fieldNameValues.Add("Locations", string.Join("|", tagListPressPlaces.Where(tag => !string.IsNullOrEmpty(tag))));
                    }

                    var parentPath = GetParentPath("/Home" + post.Link.TrimEnd('/'));
                    if (parentPath == null)
                    {
                        throw new InvalidOperationException($"parentPath could not be resolved for path: {parentPath}");
                    }

                    string parentItemId = sitecoreids.TryGetValue(parentPath, out string? resolvedParentItemId)
                        ? resolvedParentItemId
                        : string.Empty;

                    if (string.IsNullOrEmpty(parentItemId))
                    {
                        Console.WriteLine("This item is not yet in the cache, look like a special item, in this case should be exsist in Sitecore, created by hand, path=" + parentPath);
                        var specialitemTask = GetItemSecurity.Get(env, CancellationToken.None, siteroot.path + parentPath);
                        var specialitem = await specialitemTask; // Await the Task to get the actual ResultItemWithSecurity object

                        if (specialitem != null)
                        {
                            // Item exists already.
                            parentItemId = specialitem.itemIdEnclosedInBraces;
                            Console.WriteLine("Item already exists: " + specialitem.path);
                            if (!sitecoreids.ContainsKey(parentPath.TrimEnd('/')))
                            {
                                sitecoreids.Add(parentPath, parentItemId);
                            }
                            
                        }
                        if (string.IsNullOrEmpty(parentItemId))
                        {
                            throw new InvalidOperationException($"Parent item ID could not be resolved for path: {parentPath}");
                        }
                    }
                    if (isBucketItem)
                    {
                        var parentYearItemId = sitecoreids.TryGetValue(parentPath + "/" + year, out string? resolvedParentyearItemId)
                       ? resolvedParentyearItemId
                       : string.Empty;
                        if (string.IsNullOrEmpty(parentYearItemId))
                        {
                            var specialyearitemTask = GetItemSecurity.Get(env, CancellationToken.None, siteroot.path + parentPath + "/" + year);
                            var specialyearitem = await specialyearitemTask; // Await the Task to get the actual ResultItemWithSecurity object
                            if (specialyearitem != null)
                            {
                                // year Item exists already.
                                parentItemId = specialyearitem.itemIdEnclosedInBraces;
                                if (!sitecoreids.ContainsKey(parentPath + "/" + year))
                                {
                                    sitecoreids.Add(parentPath + "/" + year, parentItemId);
                                }
                            }
                            else
                            {
                                var updatedyear = await CreateFolderItem.CreateMap(env, CancellationToken.None, year, CreateFolderItem.BucketFolderID, parentItemId, language, new string[0]);
                                if (updatedyear != null)
                                {
                                    Console.WriteLine("Created year item: " + "/Home" + post.Link + "/" + year);
                                    sitecoreids.Add(parentPath + "/" + year, updatedyear.itemIdEnclosedInBraces);
                                    parentItemId = updatedyear.itemIdEnclosedInBraces;
                                }
                            }
                        } else
                        {
                            Console.WriteLine("Year Item already exists: " + "/Home" + post.Link + "/" + year);
                            parentItemId = parentYearItemId;
                        }
                    }
                    //Warning sitecore has a 100 char item name limit, so slug and SitecoreItemName  can be different..
                    var updated = await AddItem.Create(env, CancellationToken.None, post.SitecoreItemName, templateid, parentItemId, language, fieldNameValues);
                    if (updated != null)
                    {
                        Console.WriteLine("Created item: " + "/Home" + post.Link);
                        sitecoreids.Add("/Home" + post.Link.TrimEnd('/'), updated.itemIdEnclosedInBraces);
                        CreateLocalDataSources(env, blocks, updated.itemIdEnclosedInBraces,language, mediaFolderPath, post.SitecoreItemName);
                    } else {
                        Console.WriteLine("Failed to create item: " + "/Home" + post.Link);
                        Console.WriteLine("Failed to create post.SitecoreItemName: " + post.SitecoreItemName);
                    }
                }
            }
            Console.WriteLine("Imported Ended all selected records are processed. see the the above ,Logging for details ");
        }
            
    }
}

