using System.Web;

namespace SitecoreCommander.Edge
{
    internal static class EdgeHelper
    {
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
