using SitecoreCommander.Agent.Model;
using SitecoreCommander.Edge.Model;
using SitecoreCommander.Login;
using SitecoreCommander.Utils;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.Json;

namespace SitecoreCommander.Agent
{
    internal class ListJobOperations
    {
        internal static async Task<List<ListJobOperationsResponse>?> GetJob(JwtTokenResponse token, CancellationToken cancellationToken, string jobId)
        {
            string agentApiEndpoint = "https://edge-platform.sitecorecloud.io/stream/ai-agent-api/api/v1/jobs/" + jobId + "/operations";

            Console.WriteLine("Agent API Searching for operations job: " + jobId);

            using HttpClient client = new();
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token.access_token);

            using HttpResponseMessage response = await client.GetAsync(agentApiEndpoint, cancellationToken);
            
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // 404:, return null
                Console.WriteLine("JobId:" + jobId + " not found");
                return null;
            }

            string json = await response.Content.ReadAsStringAsync(cancellationToken);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            return JsonSerializer.Deserialize<List<ListJobOperationsResponse>>(json, options);
        }
    }
}
