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
            string audience = "https://api.sitecorecloud.io")
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "https://auth.sitecorecloud.io/oauth/token")
            {
                Content = new FormUrlEncodedContent(new[]
                {
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("client_secret", clientSecret),
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("audience", audience)
            })
            };

            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<JwtTokenResponse>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
    }
}
