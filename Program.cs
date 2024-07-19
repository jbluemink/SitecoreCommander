// See https://aka.ms/new-console-template for more information
using SitecoreCommander.Command;
using SitecoreCommander.Lib;

Console.WriteLine("Hello, World!");
var env = Login.GetSitecoreEnvironment();


var status = await DeleteItemLanguageVersionFromSubtree.RemoveAsync(env, "/sitecore/content/xxxx", "en");


//Console.ReadKey();
