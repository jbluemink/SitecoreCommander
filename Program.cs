// See https://aka.ms/new-console-template for more information
using SitecoreCommander.Command;
using SitecoreCommander.Lib;

Console.WriteLine("Hello, World!,  Adjust the Program.cs to do you task");
var env = Login.GetSitecoreEnvironment();

//Example DeleteItemLanguageVersionFromSubtree
//var status = await DeleteItemLanguageVersionFromSubtree.RemoveAsync(env, "/sitecore/content/xxxx", "en");

//Example MoveSubItems
var status = await MoveSubItems.MoveAsync(env, "/sitecore/content/Home/mytree", "en", "/sitecore/content/Home/tree2", new string[] {"dontmove"});


//Console.ReadKey();
