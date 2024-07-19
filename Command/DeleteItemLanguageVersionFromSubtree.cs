using SitecoreCommander.Authoring;
using SitecoreCommander.Edge.Model;
using SitecoreCommander.Edge;

namespace SitecoreCommander.Command
{
    internal static class DeleteItemLanguageVersionFromSubtree
    {
        public static async Task<bool> RemoveAsync(EnvironmentConfiguration env, string path, string language) {
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
                        List<SearchResultItem> versionItemsToDelete = new List<SearchResultItem>();
                        do
                        {
                            result = await GetItemVersionsAndDescendants.SearchPagination(env, cts.Token, subtreeroot.id, language, paginationEndCursor);
                            paginationEndCursor = result.pageOne.pageInfo.endCursor;
                            //first get all items to delete, because when you alread start with deleting the endCursor /total result can change.
                            versionItemsToDelete.AddRange( result.pageOne.results);
                        } while (result.pageOne.pageInfo.hasNext);

                        foreach (SearchResultItem item in versionItemsToDelete)
                        {
                            var deleteresult = await DeleteItemVersion.Delete(env, cts.Token, item.id, item.language.name, item.version.ToString());
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
