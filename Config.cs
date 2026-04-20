
using System.Text.Json;

namespace SitecoreCommander
{
    public class Config
    {

        private static readonly Lazy<Dictionary<string, string>> AppSettings = new(LoadAppSettings);

        private static string GetSetting(string envKey, string appSettingsKey, string defaultValue = "")
        {
            // Priority: appsettings.local.json > default value
            if (AppSettings.Value.TryGetValue(appSettingsKey, out var appValue) && !string.IsNullOrWhiteSpace(appValue))
            {
                return appValue;
            }

            return defaultValue;
        }

        private static Dictionary<string, string> LoadAppSettings()
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            var environmentName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
            if (string.IsNullOrWhiteSpace(environmentName))
            {
                environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            }

            var fileNames = new List<string> { "appsettings.Local.json" };
            if (!string.IsNullOrWhiteSpace(environmentName))
            {
                fileNames.Add($"appsettings.{environmentName}.json");
            }

            // Search in a priority list: CWD first, then AppContext.BaseDirectory, then walk up from there
            // This ensures settings are found both with 'dotnet run' (CWD=project root) and
            // when launching the .exe directly (CWD=bin/Debug/net8.0).
            var searchRoots = new List<string>();
            var cwd = Directory.GetCurrentDirectory();
            if (!string.IsNullOrWhiteSpace(cwd))
                searchRoots.Add(cwd);
            var baseDir = AppContext.BaseDirectory;
            if (!string.IsNullOrWhiteSpace(baseDir) && !searchRoots.Contains(baseDir, StringComparer.OrdinalIgnoreCase))
                searchRoots.Add(baseDir);
            // Walk up from baseDir to find the project/repo root (contains .csproj or .sln)
            var dir = new DirectoryInfo(baseDir);
            while (dir?.Parent != null)
            {
                dir = dir.Parent;
                if (dir.GetFiles("*.csproj").Length > 0 || dir.GetFiles("*.sln").Length > 0)
                {
                    if (!searchRoots.Contains(dir.FullName, StringComparer.OrdinalIgnoreCase))
                        searchRoots.Add(dir.FullName);
                    break;
                }
            }

            foreach (var fileName in fileNames)
            {
                string? fullPath = null;
                foreach (var root in searchRoots)
                {
                    var candidate = Path.Combine(root, fileName);
                    if (File.Exists(candidate))
                    {
                        fullPath = candidate;
                        break;
                    }
                }

                if (fullPath == null)
                {
                    continue;
                }

                using var stream = File.OpenRead(fullPath);
                using var document = JsonDocument.Parse(stream);
                if (document.RootElement.ValueKind != JsonValueKind.Object)
                {
                    continue;
                }

                AddJsonValues(document.RootElement, string.Empty, result);
            }

            return result;
        }

        private static void AddJsonValues(JsonElement element, string prefix, Dictionary<string, string> values)
        {
            foreach (var property in element.EnumerateObject())
            {
                var key = string.IsNullOrEmpty(prefix)
                    ? property.Name
                    : $"{prefix}:{property.Name}";

                if (property.Value.ValueKind == JsonValueKind.Object)
                {
                    AddJsonValues(property.Value, key, values);
                    continue;
                }

                if (property.Value.ValueKind == JsonValueKind.Array)
                {
                    continue;
                }

                values[key] = property.Value.ToString();
            }
        }

        // Use Sitecore CLI login for Authoring API access (path to .sitecore/user.json).
        public static string XMCloudUserJsonPath = GetSetting("SITECOMMANDER_XMCLOUD_USER_JSON_PATH", "SitecoreCommander:XMCloudUserJsonPath", string.Empty);
        // Leave empty to use the default endpoint, or set a specific --environment-name value.
        public static string EnvironmentName = GetSetting("SITECOMMANDER_ENVIRONMENT_NAME", "SitecoreCommander:EnvironmentName", @"default");


        // Sitecore API key for Edge/Preview/local CM usage.
        // See /sitecore/system/Settings/Services/API Keys.
        internal static string apikey = GetSetting("SITECOMMANDER_API_KEY", "SitecoreCommander:ApiKey");

        // See https://deploy.sitecorecloud.io/credentials/environment for JWT client credentials.

        internal static string JwtClientId = GetSetting("SITECOMMANDER_JWT_CLIENT_ID", "SitecoreCommander:JwtClientId");
        internal static string JwtClientSecret = GetSetting("SITECOMMANDER_JWT_CLIENT_SECRET", "SitecoreCommander:JwtClientSecret");

        internal static string DefaultLanguage = GetSetting("SITECOMMANDER_DEFAULT_LANGUAGE", "SitecoreCommander:DefaultLanguage", "en");

        // Values for the legacy Sitecore.Services.Client ItemService.
        // Useful for migration scenarios and older Sitecore XP versions.
        internal static string RestFullApiHostname = GetSetting("SITECOMMANDER_RESTFULL_API_HOSTNAME", "SitecoreCommander:RestFullApiHostname", "https://xmcloudcm.localhost");
        internal static string RestFullSitecoreUser = GetSetting("SITECOMMANDER_RESTFULL_SITECORE_USER", "SitecoreCommander:RestFullSitecoreUser", "admin");
        internal static string RestFullSitecorePassword = GetSetting("SITECOMMANDER_RESTFULL_SITECORE_PASSWORD", "SitecoreCommander:RestFullSitecorePassword");

        // Get any setting from appsettings by key (e.g., "ContentHub:Endpoint")
        public static string? GetAppSetting(string key)
        {
            return AppSettings.Value.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value) ? value : null;
        }

    }
}
