using SitecoreCommander.Authoring;
using SitecoreCommander.Edge.Model;
using SitecoreCommander.Edge;

namespace SitecoreCommander.Command
{
    internal static class DeleteSubItems
    {
        public static async Task<bool> DeleteAsync(EnvironmentConfiguration env, string path, string targetParentPath, string[] excludeItemNames) {
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
                    var childs = await GetItemChildren.GetAll(env, cts.Token, path);
                    if (childs == null)
                    {
                        Console.WriteLine("path not exist or no childs:" + path);
                    }
                    else
                    {
                        foreach (var item in childs)
                        {
                            if (excludeItemNames.Contains(item.name))
                            {
                                Console.WriteLine("ignore child -" + item.name);
                            } else {
                                Console.WriteLine("found child -" + item.name);
                                var result = await DeleteItem.Delete(env, cts.Token, item.path);
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
    }
}
