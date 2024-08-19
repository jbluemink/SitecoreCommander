// See https://aka.ms/new-console-template for more information
using SitecoreCommander.Authoring;
using SitecoreCommander.Command;
using SitecoreCommander.Lib;
using SitecoreCommander.RESTful;

Console.WriteLine("Hello, World!,  Adjust the Program.cs to do you task");
var env = Login.GetSitecoreEnvironment();

//Example DeleteItemLanguageVersionFromSubtree
//var status = await DeleteItemLanguageVersionFromSubtree.RemoveAsync(env, "/sitecore/content/xxxx", "en");

//Example MoveSubItems
//var status = await MoveSubItems.MoveAsync(env, "/sitecore/content/Home/mytree", "en", "/sitecore/content/Home/tree2", new string[] {"dontmove"});

//Example migrate item from sitecore with REST api to XM Cloud
var restApiCookies = SscItemService.SourceLogIn();
var sourceItem = SscItemService.GetItem("/sitecore/content/Home/test", restApiCookies, "en");
const string SxaHeadlessPageTemplateId = "{4829027E-F126-4192-ACE6-F0F2E3BE2A26}";
Guid HomeGuid = Guid.Parse("{110D559F-DEA5-42EA-9C1C-8A5DF7E70EF9}");
// place the sourceItem below Home and use template SxaHeadlessPageTemplateId beside language "en", try to add "de" and "nl" version if exist.
// adjust the CreateMigratedSxaPage to map the fields you need, and write logic for converting the layout. 
var status = await CreateMigratedSxaPage.CreateLabelPageItem(env, restApiCookies, CancellationToken.None, "migratedHome2", HomeGuid, sourceItem, SxaHeadlessPageTemplateId, new string[] { "de", "nl" });


//Console.ReadKey();
