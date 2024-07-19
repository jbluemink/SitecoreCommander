
namespace SitecoreCommander
{
    internal class Config
    {
        //Use Sitecore CLI for Login into Sitecore and use the Authoring API, The path to the user.json
        internal static string XMCloudUserJsonPath = @"C:\projects\xmcloud-foundation-head\.sitecore\user.json";
        //leave EnvironmentName empty for default or use the --environment-name from the dotnet sitecore login
        internal static string EnvironmentName = @"";


        //api key Sitecore for the Edge, Preview, Local on the CM, no publish nessecary to Live Edge, same data as Authoring API.
        //Seev /sitecore/system/Settings/Services/API Keys
        internal static string apikey = "{11111111-1111-1111-1111-111111111111}";

        internal static string DefaultLanguage = "en";

    }
}
