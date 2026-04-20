using System.Web;

namespace SitecoreCommander.Edge
{
    internal static class EdgeHelper
    {
        internal static string ResolveApiKey()
        {
            // Single API key source for all Sitecore GraphQL/Edge calls.
            // Normalize common "{GUID}" config format to raw GUID.
            var apiKey = Config.apikey;
            return (apiKey ?? string.Empty).Trim().Trim('{', '}');
        }

        internal static string FormatGraphQlErrorsWithGuidance(IEnumerable<SitecoreCommander.Login.Request.GraphQLError> errors)
        {
            var messages = errors
                .Select(x => x.Message)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();

            var details = string.Join("\n", messages.Select(m => $"  - {m}"));
            var hasApiKeyIssue = messages.Any(m =>
                m.Contains("api key", StringComparison.OrdinalIgnoreCase) ||
                m.Contains("sc_apikey", StringComparison.OrdinalIgnoreCase));

            if (hasApiKeyIssue)
            {
                return details + "\n  - Configure SitecoreCommander:ApiKey in appsettings.Local.json with the Sitecore item API key from /sitecore/system/Settings/Services/API Keys.";
            }

            return details;
        }

        internal static string QueryFormatRemoveIfEmpty(string name, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }
            return name + ": \""+ HttpUtility.JavaScriptStringEncode(value) + "\"";
        }

        internal static string QueryFormatIntRemoveIfEmpty(string name, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }
            return name + ": " + value;
        }
    }
}
