using SitecoreCommander.Agent.Model;
using SitecoreCommander.Edge.Model;
using SitecoreCommander.Login;
using SitecoreCommander.Utils;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.Json;

namespace SitecoreCommander.Agent
{
    internal class RevertJob
    {
        internal static async Task<RevertJobRespons?> Revert(JwtTokenResponse token, CancellationToken cancellationToken, string jobId)
        {
            Console.WriteLine("Agent API try to revert job: " + jobId);
            string agentApiEndpoint = "https://edge-platform.sitecorecloud.io/stream/ai-agent-api/api/v1/jobs/" + jobId + "/revert";

            using HttpClient client = new();
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token.access_token);

            using HttpResponseMessage request = await client.PostAsync(agentApiEndpoint, null, cancellationToken);
            string json = await request.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            return JsonSerializer.Deserialize<RevertJobRespons>(json, options);
        }
    }
}
