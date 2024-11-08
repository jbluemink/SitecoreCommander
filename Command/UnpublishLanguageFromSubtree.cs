using SitecoreCommander.Authoring;
using SitecoreCommander.Edge.Model;
using SitecoreCommander.Edge;

namespace SitecoreCommander.Command
{
    internal static class UnpublishLanguageFromSubtree
    {
        public static async Task<bool> EditAsync(EnvironmentConfiguration env, string path, string language)
        {
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
                    var subtreeroot = await GetItem.GetSitecoreItem(env, cts.Token, path, language);
                    if (subtreeroot == null)
                    {
                        Console.WriteLine("path not exist:" + path + " in language:" + language);
                    }
                    else
                    {
                        SearchPaginationItems result;
                        string paginationEndCursor = "";
                        List<SearchResultItem> versionItemsToEdit = new List<SearchResultItem>();
                        do
                        {
                            result = await GetItemVersionsAndDescendants.SearchPagination(env, cts.Token, subtreeroot.id, language, paginationEndCursor);
                            paginationEndCursor = result.pageOne.pageInfo.endCursor;
                            //first get all items and versions, to edit, because when you alread start editing the endCursor /total result can change.
                            versionItemsToEdit.AddRange(result.pageOne.results);
                        } while (result.pageOne.pageInfo.hasNext);

                        foreach (SearchResultItem item in versionItemsToEdit)
                        {
                            var editresult = await UpdateItemVersionUnpublish.UpdateItem(env, cts.Token, item.id, item.version, "1", item.language.name);
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
    }
}
