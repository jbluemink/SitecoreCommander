using SitecoreCommander.Utils;
using System.Net;
using System.Text.Json;

namespace SitecoreCommander.Agent
{
    internal static class AgentApiResponseHelper
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        internal static T? DeserializeOrThrow<T>(HttpResponseMessage response, string body, string endpoint)
        {
            body ??= string.Empty;

            if (response.StatusCode == HttpStatusCode.NotFound)
                return default;

            if (!response.IsSuccessStatusCode)
            {
                var snippet = BuildSnippet(body);
                var message =
                    $"Agent API request failed: {(int)response.StatusCode} ({response.ReasonPhrase})\n" +
                    $"Endpoint: {endpoint}\n" +
                    $"Response: {snippet}\n" +
                    "This usually means the token is not valid for the cloud Agent endpoint (audience/scope/tenant mismatch).";

                throw new InvalidOperationException(message);
            }

            var trimmed = body?.TrimStart() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(trimmed))
            {
                throw new InvalidOperationException($"Agent API returned an empty body for endpoint '{endpoint}'.");
            }

            var first = trimmed[0];
            if (first != '{' && first != '[')
            {
                var snippet = BuildSnippet(body);
                throw new InvalidOperationException(
                    $"Agent API returned non-JSON content for endpoint '{endpoint}'. Response: {snippet}");
            }

            try
            {
                return JsonSerializer.Deserialize<T>(body, JsonOptions);
            }
            catch (JsonException ex)
            {
                var snippet = BuildSnippet(body);
                throw new InvalidOperationException(
                    $"Agent API response parsing failed for endpoint '{endpoint}'. Response: {snippet}", ex);
            }
        }

        internal static async Task LogTokenDiagnosticsAsync(string operation, string endpoint, string scope)
        {
            await SimpleLogger.LogAsync($"[AgentApi] {operation} | endpoint={endpoint} | tokenScope={scope}");
        }

        private static string BuildSnippet(string body)
        {
            if (string.IsNullOrWhiteSpace(body))
                return "<empty>";

            var oneLine = body.Replace("\r", " ").Replace("\n", " ").Trim();
            return oneLine.Length <= 220 ? oneLine : oneLine.Substring(0, 220) + "...";
        }
    }
}
