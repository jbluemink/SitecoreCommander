using SitecoreCommander.Authoring;
using SitecoreCommander.Edge.Model;
using WordpressXmlImport;

namespace SitecoreCommander.Wordpress
{
    class WordpressImport
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


        public async Task ImportPostsAsync(EnvironmentConfiguration env, ResultGetItem siteroot, string language, string templateid, string filepath)
        {
            var importer = new ExtendedXmlImporter();
            var sortePosts = importer.LoadPosts(filepath);
            var sitecoreids = new Dictionary<string, string>();
            sitecoreids.Add("/", siteroot.itemIdEnclosedInBraces);
            foreach (var post in sortePosts)
            {
                Console.WriteLine($"[{post.PostType}] {post.Link} {post.Title} ({post.PostDate}): {post.Slug}");
                var itemTask = GetItemSecurity.Get(env, CancellationToken.None, siteroot.path + "/Home" + post.Link);
                var item = await itemTask; // Await the Task to get the actual ResultItemWithSecurity object

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
                    // Create the item
                    var fieldNameValues = new Dictionary<string, string>
                    {
                        { "Title", post.Title },
                        { templateid == "{76036F5E-CBCE-46D1-AF0A-4143F9B557AA}" ? "Text" : "Content", post.Content },
                        { "__Created", post.PostDate.ToString("yyyyMMdd'T'HHmmss'Z'") },
                    };
                    var parentPath = GetParentPath("/Home" + post.Link.TrimEnd('/'));
                    if (parentPath == null)
                    {
                        throw new InvalidOperationException($"parentPath could not be resolved for path: {parentPath}");
                    }
                    string parentItemId = sitecoreids.TryGetValue(parentPath, out string? resolvedParentItemId)
                        ? resolvedParentItemId
                        : siteroot.itemIdEnclosedInBraces;

                    if (parentItemId == null)
                    {
                        throw new InvalidOperationException($"Parent item ID could not be resolved for path: {parentPath}");
                    }

                    var updated = await AddItem.Create(env, CancellationToken.None, post.Slug, templateid, parentItemId, language, fieldNameValues);
                    if (updated != null)
                    {
                        Console.WriteLine("Created item: " + "/Home" + post.Link);
                        sitecoreids.Add("/Home" + post.Link.TrimEnd('/'), updated.itemIdEnclosedInBraces);
                    }
                }
            }
        }
    }
}

