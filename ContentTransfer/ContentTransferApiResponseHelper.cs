using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SitecoreCommander.ContentTransfer
{
    internal static class ContentTransferApiResponseHelper
    {
        internal static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };

        internal static T? DeserializeOrThrow<T>(HttpResponseMessage response, string? body, string endpoint)
        {
            var responseBody = body ?? string.Empty;

            if (response.StatusCode == HttpStatusCode.NotFound)
                return default;

            if (!response.IsSuccessStatusCode)
                ThrowRequestFailed(response, responseBody, endpoint);

            if (string.IsNullOrWhiteSpace(responseBody))
                return default;

            try
            {
                return JsonSerializer.Deserialize<T>(responseBody, JsonOptions);
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException(
                    $"Content Transfer API response parsing failed for endpoint '{endpoint}'. Response: {BuildSnippet(responseBody)}",
                    ex);
            }
        }

        internal static void EnsureSuccess(HttpResponseMessage response, string? body, string endpoint)
        {
            if (response.StatusCode == HttpStatusCode.NotFound)
                return;

            if (!response.IsSuccessStatusCode)
                ThrowRequestFailed(response, body ?? string.Empty, endpoint);
        }

        internal static void ThrowRequestFailed(HttpResponseMessage response, string body, string endpoint)
        {
            throw new InvalidOperationException(
                $"Content Transfer API request failed: {(int)response.StatusCode} ({response.ReasonPhrase})\n" +
                $"Endpoint: {endpoint}\n" +
                $"Response: {BuildSnippet(body)}");
        }

        private static string BuildSnippet(string body)
        {
            if (string.IsNullOrWhiteSpace(body))
                return "<empty>";

            var oneLine = body.Replace("\r", " ").Replace("\n", " ").Trim();
            return oneLine.Length <= 300 ? oneLine : oneLine[..300] + "...";
        }
    }
}