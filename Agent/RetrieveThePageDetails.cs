using SitecoreCommander.Agent.Model;
using SitecoreCommander.Edge.Model;
using SitecoreCommander.Login;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.Json;

namespace SitecoreCommander.Agent
{
    internal class RetrieveThePageDetails
    {
        internal static async Task<RetrieveThePageDetailsResponse?> GetItemById(JwtTokenResponse token, CancellationToken cancellationToken, string pageId)
        {
            string agentApiEndpoint = "https://edge-platform.sitecorecloud.io/stream/ai-agent-api/api/v1/content/" + pageId;

            Console.WriteLine("Agent API Get page details: " + pageId);

            using HttpClient client = new();
            client.DefaultRequestHeaders.Add("x-sc-job-id", "commander-job-sites" + RandomNumberGenerator.GetInt32(0, int.MaxValue));
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token.access_token);

            using HttpResponseMessage response = await client.GetAsync(agentApiEndpoint, cancellationToken);
            string json = await response.Content.ReadAsStringAsync(cancellationToken);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            return JsonSerializer.Deserialize<RetrieveThePageDetailsResponse>(json, options);
        }
    }
}
