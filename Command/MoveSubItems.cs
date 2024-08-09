using SitecoreCommander.Authoring;
using SitecoreCommander.Edge.Model;
using SitecoreCommander.Edge;

namespace SitecoreCommander.Command
{
    internal static class MoveSubItems
    {
        public static async Task<bool> MoveAsync(EnvironmentConfiguration env, string path, string language, string targetParentPath, string[] excludeItemNames) {
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
                    var childs = await GetChilderen.Get(env, cts.Token, path, language);
                    if (childs == null)
                    {
                        Console.WriteLine("path not exist or no childs:" + path + " in language:" + language);
                    }
                    else
                    {
                        foreach (ResultGetItem item in childs)
                        {
                            if (excludeItemNames.Contains(item.name))
                            {
                                Console.WriteLine("ignore child -" + item.name);
                            } else {
                                Console.WriteLine("found child -" + item.name);
                                var result = await MoveItem.Move(env, cts.Token, item.path, targetParentPath);
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
