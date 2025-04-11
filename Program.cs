﻿using RaiXpToCloudMigrator.XmCloud;
using SitecoreCommander;
using SitecoreCommander.Authoring;
using SitecoreCommander.Command;
using SitecoreCommander.Edge;
using SitecoreCommander.Edge.Model;
using SitecoreCommander.Lib;
using SitecoreCommander.RESTful;
using SitecoreCommander.Wordpress;

Console.WriteLine("Hello, World!,  Adjust the Program.cs to do you task");
var env = Login.GetSitecoreEnvironment();

//Example import from WordPress XML file
var language = "en";
var siteroot = await GetItem.GetSitecoreItem(env, CancellationToken.None, "/sitecore/content/Home", language);
if (siteroot == null)
{
    Console.WriteLine("Site root not found");
    return;
}
var wp = new WordpressImport();
await wp.ImportPostsAsync(env, siteroot, language, "{76036F5E-CBCE-46D1-AF0A-4143F9B557AA}", @"C:\projects\SitecoreCommander\WordPress-example.xml");


//Example create multiple items
//  var parent = await GetItem.GetSitecoreItem(env, CancellationToken.None, "/sitecore/content/Home/test");
//  if (parent != null)
//  {
//    for (int i = 0; i < 10; i++)
//    {
//        var fieldNameValues = new Dictionary<string, string>
//        {
//            { "Title", "Test "+i },
//            { "Text", "Test item created with SitecoreCommander" }
//        };
//        var updated = await AddItem.Create(env, CancellationToken.None, "testitem"+i, AddItem.SampleItemTemplateID, parent.id, "en", fieldNameValues);
//    }
//   }

// Example: Set all items in a tree as unpublishable for a specific language (the language should exist for the root item).
//var result = await UnpublishLanguageFromSubtree.EditAsync(env, "/sitecore/content/Home","es");


//Example move al items in a folder to subfolders based on the created month
//var status = await MoveSubItemsToMonthFolders.MoveAsync(env, "/sitecore/media library/Project/test2", "en", CreateFolderItem.MediaFolderID);

//Example move al items in a folder to subfolders 
//var status = await MoveSubItemsToMultipleFolders.MoveAsync(env, "/sitecore/media library/Project/test1", "en", CreateFolderItem.MediaFolderID,50,100);


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
//var status = await MoveSubItems.MoveAsync(env, "/sitecore/content/Home/test", "/sitecore/content/Home/test2", new string[] {"dontmove"});

//Example migrate item from sitecore with REST api to XM Cloud
//var restApiCookies = SscItemService.SourceLogIn();
//var sourceItem = SscItemService.GetItem("/sitecore/content/Home/test", restApiCookies, "en");
//const string SxaHeadlessPageTemplateId = "{4829027E-F126-4192-ACE6-F0F2E3BE2A26}";
//Guid HomeGuid = Guid.Parse("{110D559F-DEA5-42EA-9C1C-8A5DF7E70EF9}");
//place the sourceItem below Home and use template SxaHeadlessPageTemplateId beside language "en", try to add "de" and "nl" version if exist.
//adjust the CreateMigratedSxaPage to map the fields you need, and write logic for converting the layout. 
//var status = await CreateMigratedSxaPage.CreateLabelPageItem(env, restApiCookies, CancellationToken.None, "migratedHome2", HomeGuid, sourceItem, SxaHeadlessPageTemplateId, new string[] { "de", "nl" });


//Console.ReadKey();
