﻿using RaiXpToCloudMigrator.XmCloud;
using SitecoreCommander;
using SitecoreCommander.Authoring;
using SitecoreCommander.Authoring.Model;
using SitecoreCommander.Command;
using SitecoreCommander.Edge;
using SitecoreCommander.Edge.Model;
using SitecoreCommander.Lib;
using SitecoreCommander.RESTful;
using SitecoreCommander.WordPress;

Console.WriteLine("Hello, World!,  Adjust the Program.cs to do you task");
var env = Login.GetSitecoreEnvironment();

//Example replace field in subtree
var result = ReplaceFieldFromSubtree.ReplaceAsync(env, "/sitecore/content", "en", "Title", "&nbsp;", " ", "Sample Item").GetAwaiter().GetResult();


//Example upload media
//var upload = await AddMedia.Create(env, CancellationToken.None, "/sitecore/media library/Project/test3/hello/test", "en", "test", @"C:\projects\SitecoreCommander\Test.jpg");

//Example import from WordPress XML file
//Install Wordpress/Demo headless site wordpress import.zip  Sitecore Package For Site Structure and page templsate
/*var language = "en";
var sitecoreSite = await GetItem.GetSitecoreItem(env, CancellationToken.None, "/sitecore/content/Demo/Site", language);
var siteHome = await GetItem.GetSitecoreItem(env, CancellationToken.None, "/sitecore/content/Demo/Site/Home", language);
if (siteHome == null)
{
    Console.WriteLine("Site Home item not found");
     return;
}
var wp = new WordPressSampleImport();

await wp.ImportPostsAsync(
    env: env,
    siteroot: sitecoreSite,
    language: language,
    defaulttemplateid: Templates.PageTemplateGuid,
    tagFolderPath: "/sitecore/content/Demo/Site/Data/Tags",
    mediaFolderPath: "/sitecore/media library/Project/Demo/Site",
    filepath: @"./WordPress-example.xml",
    overwrite: true
);*/

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
