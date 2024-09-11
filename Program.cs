using RaiXpToCloudMigrator.XmCloud;
using SitecoreCommander.Authoring;
using SitecoreCommander.Command;
using SitecoreCommander.Edge;
using SitecoreCommander.Lib;
using SitecoreCommander.RESTful;

Console.WriteLine("Hello, World!,  Adjust the Program.cs to do you task");
var env = Login.GetSitecoreEnvironment();


//Example move al items in a folder to subfolders based on the created month
//var status = await MoveSubItemsToMonthFolders.MoveAsync(env, "/sitecore/media library/Project/RAI Amsterdam xmc/Aquatech/Aquatech/news/2022", "en", CreateFolderItem.MediaFolderID);


//Example remove al item security from items in a subtree
//var status = await DeleteSecurityFromSubtree.RemoveAsync(env, "/sitecore/content/Home", "en");


//Example iterate through all sites and update the security of the home item 
//var sites = GetEdgeSites.Get(env, CancellationToken.None);
//foreach (var site in sites.Result)
//{
//    var homeItem = await GetItem.GetSitecoreItem(env, CancellationToken.None, site.rootPath);
//    if (homeItem != null)
//   {
//        Console.WriteLine("set right for site " + site.name);
//        //var updated = await UpdateItemSecurity.UpdateItem(env, CancellationToken.None, homeItem.id, "ar|sitecore\\Developer|pd|+item:read|pe|+item:write|+item:read|", "en");
//        var updated = await UpdateItemSecurity.UpdateItem(env, CancellationToken.None, homeItem.id, "", "en");
//    }
//}


//Example DeleteItemLanguageVersionFromSubtree
//var status = await DeleteItemLanguageVersionFromSubtree.RemoveAsync(env, "/sitecore/content/xxxx", "en");

//Example MoveSubItems
//var status = await MoveSubItems.MoveAsync(env, "/sitecore/content/Home/mytree", "en", "/sitecore/content/Home/tree2", new string[] {"dontmove"});

//Example migrate item from sitecore with REST api to XM Cloud
//var restApiCookies = SscItemService.SourceLogIn();
//var sourceItem = SscItemService.GetItem("/sitecore/content/Home/test", restApiCookies, "en");
//const string SxaHeadlessPageTemplateId = "{4829027E-F126-4192-ACE6-F0F2E3BE2A26}";
//Guid HomeGuid = Guid.Parse("{110D559F-DEA5-42EA-9C1C-8A5DF7E70EF9}");
// place the sourceItem below Home and use template SxaHeadlessPageTemplateId beside language "en", try to add "de" and "nl" version if exist.
// adjust the CreateMigratedSxaPage to map the fields you need, and write logic for converting the layout. 
//var status = await CreateMigratedSxaPage.CreateLabelPageItem(env, restApiCookies, CancellationToken.None, "migratedHome2", HomeGuid, sourceItem, SxaHeadlessPageTemplateId, new string[] { "de", "nl" });


//Console.ReadKey();
