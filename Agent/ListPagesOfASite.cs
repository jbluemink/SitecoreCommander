using SitecoreCommander.Agent.Model;
using SitecoreCommander.Edge.Model;
using SitecoreCommander.Login;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.Json;

namespace SitecoreCommander.Agent
{
    internal class ListSitListPagesOfASitees
    {
        internal static async Task<ListPagesOfASiteResponse?> GetPages(JwtTokenResponse token, CancellationToken cancellationToken, string siteName, string language)
        {
            string agentApiEndpoint = "https://edge-platform.sitecorecloud.io/stream/ai-agent-api/api/v1/sites/"+siteName+"/pages?language="+language;

            Console.WriteLine("Agent API Searching for Pages sitename: " + siteName);

            using HttpClient client = new();
            client.DefaultRequestHeaders.Add("x-sc-job-id", "commander-job-sites" + RandomNumberGenerator.GetInt32(0, int.MaxValue));
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token.access_token);

            using HttpResponseMessage response = await client.GetAsync(agentApiEndpoint, cancellationToken);
            string json = await response.Content.ReadAsStringAsync(cancellationToken);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

             return JsonSerializer.Deserialize<ListPagesOfASiteResponse>(json, options);
        }
    }
}
