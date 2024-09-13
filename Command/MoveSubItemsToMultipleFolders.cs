using RaiXpToCloudMigrator.XmCloud;
using SitecoreCommander.Authoring;
using SitecoreCommander.Authoring.Model;

namespace SitecoreCommander.Command
{
    internal static class MoveSubItemsToMultipleFolders
    {
        public static async Task<bool> MoveAsync(EnvironmentConfiguration env, string path, string language, string FolderTemplateId, int maxitems, int startFolderNumber) {
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
                        int number;
                        int subCount = 0;
                        int foldercount = startFolderNumber;
                        var subFolder = await GetFolderPathOrCreate(env, cts.Token, rootItem, FolderTemplateId, foldercount, language);
                        foreach (var item in childs)
                        {
                            Console.WriteLine("found child -" + item.name );
                            if (!int.TryParse(item.name, out number))
                            {
                                subCount++;
                                if (subCount > maxitems)
                                {
                                    foldercount++;
                                    subCount = 1;
                                    subFolder = await GetFolderPathOrCreate(env, cts.Token, rootItem, FolderTemplateId, foldercount, language);

                                }
                                var result = await Authoring.MoveItem.Move(env, cts.Token, item.path, subFolder);
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

        private static async Task<string> GetFolderPathOrCreate(EnvironmentConfiguration env, CancellationToken cancellationToken, ResultItemWithSecurity rootItem, string FolderTemplateId, int foldercount, string language)
        {
            var subFolderPath = rootItem.path + "/" + foldercount.ToString();

            // Otherwise, perform the actual logic
            var subFolder = await GetItemSecurity.Get(env, cancellationToken, subFolderPath);

            if (subFolder != null)
            {
                return subFolder.path;
            }

            var newFolder = await CreateFolderItem.CreateMap(env, cancellationToken, foldercount.ToString(), FolderTemplateId, rootItem.itemId, language, new string[] { });

            return subFolderPath;
        }
    }
}
