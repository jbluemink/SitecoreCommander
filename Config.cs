
namespace SitecoreCommander
{
    internal class Config
    {

        //Use Sitecore CLI for Login into Sitecore and use the Authoring API, The path to the user.json
        //internal static string XMCloudUserJsonPath = @"C:\projects\raixm\.sitecore\user.json";
        internal static string XMCloudUserJsonPath = @"C:\projects\vanwijnen\.sitecore\user.json";
        //leave EnvironmentName empty for default or use the --environment-name from the dotnet sitecore login
        internal static string EnvironmentName = @"default";


        //api key Sitecore for the Edge, Preview, Local on the CM, no publish nessecary to Live Edge, same data as Authoring API.
        //See /sitecore/system/Settings/Services/API Keys
        internal static string apikey = "{F2782F9C-5242-46A4-84B3-10189026BB74}";
        //internal static string apikey = "{43C06134-8D34-4895-8A6B-880638DBAB48}";
        //internal static string apikey = "{4C6772F2-3D4E-48A6-9B1E-4AA7A7DB794F}"; //van-wijnen-tst
        //internal static string apikey = "{D7128498-85B5-41B7-9B4B-09752FE447E2}"; //van wijnern acc

        internal static string DefaultLanguage = "en";

        //values for the old Sitecore.Services.Client The ItemService
        //This API might be useful for migration,
        //because it has been around for a long time and even old Sitecore XP versions have this API without the need to install anything extra.
        internal static string RestFullApiHostname = "https://cm-prd.raicore.com";
        internal static string RestFullSitecoreUser = "xmcloudmigrator";
        internal static string RestFullSitecorePassword = "HopHop55CLO77!9+p";

    }
}
