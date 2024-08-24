using SitecoreCommander.Authoring;
using SitecoreCommander.Edge.Model;
using SitecoreCommander.Edge;
using SitecoreCommander.Authoring.Model;

namespace SitecoreCommander.Command
{
    internal static class DeleteSecurityFromSubtree
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
                        SearchWithSecurity result;
                        int paginationPage = 0;
                        int pageSize = 5;
                        int totalResult = 0;
                        List<ResultItemWithSecurity> itemsToUpdate = new List<ResultItemWithSecurity>();
                        do
                        {
                            //not possible currently to filter on security, so get all items and filter in code
                             result = await GetItemSecurityAndDescendants.SearchPagination(env, cts.Token, subtreeroot.id, pageSize, paginationPage, language);
                             paginationPage++;
                            totalResult = result.search.totalCount;
                            //first get all items to modify, because when you alread start editing it might effect the pagination.
                            if (result.search.results != null)
                            {
                                foreach(var item in result.search.results)
                                {
                                    if (!string.IsNullOrEmpty(item.innerItem.security.value))
                                    {
                                        itemsToUpdate.Add(item.innerItem);
                                    }
                                }
                            }
                        } while (totalResult > paginationPage* pageSize);

                        foreach (ResultItemWithSecurity item in itemsToUpdate)
                        {
                            var updateresult = await UpdateItemSecurity.UpdateItem(env, cts.Token, item.itemId, "",item.language.name);
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
