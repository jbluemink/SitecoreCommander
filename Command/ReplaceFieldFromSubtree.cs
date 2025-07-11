using SitecoreCommander.Authoring;
using SitecoreCommander.Edge.Model;
using SitecoreCommander.Edge;
using SitecoreCommander.Authoring.Model;

namespace SitecoreCommander.Command
{
    internal static class ReplaceFieldFromSubtree
    {
        public static async Task<bool> ReplaceAsync(EnvironmentConfiguration env, string path, string language, string fieldname, string replaceOld, string replaceNew, string? templateName = null)
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
                        SearchWithFieldResult result;
                        int paginationPage = 0;
                        int pageSize = 25;
                        int totalResult = 0;
                        List<ResultItemWithField> itemsToUpdate = new List<ResultItemWithField>();
                        do
                        {
                            //not possible currently to filter on security, so get all items and filter in code
                            result = await GetItemFieldAndDescendants.SearchPagination(env, cts.Token, subtreeroot.id, fieldname, pageSize, paginationPage, language, templateName);
                            paginationPage++;
                             totalResult = result.search.totalCount;
                            //first get all items to modify, because when you alread start editing it might effect the pagination.
                            if (result.search.results != null)
                            {
                                foreach (var item in result.search.results)
                                {
                                    if (item.innerItem.fieldvalue != null && !string.IsNullOrEmpty(item.innerItem.fieldvalue.value) && item.innerItem.fieldvalue.value.Contains(replaceOld))
                                    {
                                        itemsToUpdate.Add(item.innerItem);
                                    }
                                }
                            }
                        } while (totalResult > paginationPage * pageSize);

                        foreach (ResultItemWithField item in itemsToUpdate)
                        {
                            Dictionary<string, string> fields = new Dictionary<string, string>();
                            fields.Add(fieldname, item.fieldvalue.value.Replace(replaceOld, replaceNew));
                            var updateresult = await UpdateItem.Update(env, cts.Token, item.itemId, item.language.name, fields);
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
