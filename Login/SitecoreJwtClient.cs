using System.Net.Http.Headers;
using System.Text.Json;
namespace SitecoreCommander.Login
{
    public static class SitecoreJwtClient
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        public static async Task<JwtTokenResponse> GetJwtAsync()
        {
            var token =  await GetJwtAsync(
                Config.JwtClientId,
                Config.JwtClientSecret,
                "https://api.sitecorecloud.io");
            if (token == null)
            {
                throw new InvalidOperationException("Failed to obtain JWT token.");
            }
            return token;
        }
        public static async Task<JwtTokenResponse?> GetJwtAsync(
            string clientId,
            string clientSecret,
            string audience = "https://api.sitecorecloud.io",
            string authority = "https://auth.sitecorecloud.io/oauth/token")
        {
            var tokenEndpoint = "https://auth.sitecorecloud.io/oauth/token";
            var request = new HttpRequestMessage(HttpMethod.Post, tokenEndpoint)
            {
                Content = new FormUrlEncodedContent(new[]
                {
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("client_secret", clientSecret),
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("audience", audience),
                new KeyValuePair<string, string>("authority", authority)
            })
            };

            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await _httpClient.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                var message =
                    "JWT authentication failed while requesting a Sitecore token.\n" +
                    $"HTTP status: {(int)response.StatusCode} ({response.ReasonPhrase})\n" +
                    $"Token endpoint: {tokenEndpoint}\n" +
                    $"Audience: {audience}\n" +
                    $"Authority parameter: {authority}\n" +
                    $"Configured environment: {Config.EnvironmentName}\n" +
                    $"Configured client id: {Mask(clientId)}\n" +
                    $"Response body: {TrimForLog(json)}\n" +
                    "Check values for SITECOMMANDER_JWT_CLIENT_ID and SITECOMMANDER_JWT_CLIENT_SECRET in .env or appsettings.Local.json.";

                throw new HttpRequestException(message, null, response.StatusCode);
            }

            return JsonSerializer.Deserialize<JwtTokenResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }

        private static string Mask(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "<empty>";
            }

            if (value.Length <= 6)
            {
                return "***";
            }

            return value[..3] + "***" + value[^3..];
        }

        private static string TrimForLog(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "<empty>";
            }

            var singleLine = value.Replace("\r", " ").Replace("\n", " ").Trim();
            const int maxLen = 500;
            if (singleLine.Length <= maxLen)
            {
                return singleLine;
            }

            return singleLine[..maxLen] + "...";
        }
    }
}
