
namespace SitecoreCommander
{
    internal class Config
    {

        //Use Sitecore CLI for Login into Sitecore and use the Authoring API, The path to the user.json
        internal static string XMCloudUserJsonPath = @"C:\projects\xxxxx\.sitecore\user.json";
        //leave EnvironmentName empty for default or use the --environment-name from the dotnet sitecore login
        internal static string EnvironmentName = @"default";


        //api key Sitecore for the Edge, Preview, Local on the CM, no publish nessecary to Live Edge, same data as Authoring API.
        //See /sitecore/system/Settings/Services/API Keys
        internal static string apikey = "{F2782F9C-5242-46A4-84B3-10189026BB74}";

        //see https://deploy.sitecorecloud.io/credentials/environment
        internal static string JwtClientId = "xxxxxx";
        internal static string JwtClientSecret = "xxxxxxxxxxx_xx_xxxxx-xxxxxx";

        internal static string DefaultLanguage = "en";

        //values for the old Sitecore.Services.Client The ItemService
        //This API might be useful for migration,
        //because it has been around for a long time and even old Sitecore XP versions have this API without the need to install anything extra.
        internal static string RestFullApiHostname = "https://xmcloudcm.localhost";
        internal static string RestFullSitecoreUser = "admin";
        internal static string RestFullSitecorePassword = "b";

    }
}
