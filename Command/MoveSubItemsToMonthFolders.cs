using SitecoreCommander.Edge;
using RaiXpToCloudMigrator.XmCloud;
using SitecoreCommander.Authoring;
using SitecoreCommander.Authoring.Model;

namespace SitecoreCommander.Command
{
    internal static class MoveSubItemsToMonthFolders
    {
        public static async Task<bool> MoveAsync(EnvironmentConfiguration env, string path, string language, string FolderTemplateId) {
            using (var cts = new CancellationTokenSource())
            {
                ConsoleCancelEventHandler handler = (o, e) =>
                {
                    Console.WriteLine("Cancelling...");
                    cts.CancelAfter(0);
                    e.Cancel = true;
                };
                Console.CancelKeyPress += handler;
                try
                {
                    var rootItem = await GetItemSecurity.Get(env, cts.Token, path);
                    var childs = await GetItemChildren.GetAll(env, cts.Token, path);
                    if (childs == null)
                    {
                        Console.WriteLine("path not exist or no childs:" + path );
                    }
                    else
                    {
                        List<string> monthValues = new List<string> { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12" };
                        foreach (var item in childs)
                        {
                            Console.WriteLine("found child -" + item.name + " month:"+item.CreatedDateTime?.Month);
                            if (item.CreatedDateTime != null && !monthValues.Contains(item.name)) {
                                var monthFolder = await GetMonthPathOrCreate(env, cts.Token, rootItem, FolderTemplateId, item.CreatedDateTime?.Month ?? 0, language);
                                var result = await Authoring.MoveItem.Move(env, cts.Token, item.path, monthFolder);
                            }
                        }
                    }
                }
                finally
                {
                    Console.CancelKeyPress -= handler;
                }
            }
            return true;
        }

        // Static cache to store month paths
        private static Dictionary<string, string> cache = new Dictionary<string, string>();
        private static async Task<string> GetMonthPathOrCreate(EnvironmentConfiguration env, CancellationToken cancellationToken, ResultItemWithSecurity rootItem, string FolderTemplateId, int month, string language)
        {
            // Use monthFolderPath as the cache key
            var monthFolderPath = rootItem.path + "/" + month.ToString();

            // Check if the result is already in cache
            if (cache.TryGetValue(monthFolderPath, value: out string cachedMonthFolderPath))
            {
                return cachedMonthFolderPath;
            }

            // Otherwise, perform the actual logic
            var monthFolder = await GetItemSecurity.Get(env, cancellationToken, monthFolderPath);

            if (monthFolder != null)
            {
                // Add to cache before returning
                cache[monthFolderPath] = monthFolder.path;
                return monthFolder.path;
            }

            var newFolder = await CreateFolderItem.CreateMap(env, cancellationToken, month.ToString(), FolderTemplateId, rootItem.itemId, language, new string[] { });

            // Add to cache after creating the new folder
            cache[monthFolderPath] = monthFolderPath;
            return monthFolderPath;
        }
    }
}
