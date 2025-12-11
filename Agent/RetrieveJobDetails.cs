using SitecoreCommander.Agent.Model;
using SitecoreCommander.Edge.Model;
using SitecoreCommander.Login;
using SitecoreCommander.Utils;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.Json;

namespace SitecoreCommander.Agent
{
    internal class RetrieveJobDetails
    {
        internal static async Task<RetrieveJobDetailsRespons?> GetJob(JwtTokenResponse token, CancellationToken cancellationToken, string jobId)
        {
            string agentApiEndpoint = "https://edge-platform.sitecorecloud.io/stream/ai-agent-api/api/v1/jobs/" + jobId;

            Console.WriteLine("Agent API Searching for job: " + jobId);

            using HttpClient client = new();
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token.access_token);

            using HttpResponseMessage response = await client.GetAsync(agentApiEndpoint, cancellationToken);
            string json = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // 404:, return null
                Console.WriteLine("JobId:" + jobId + " not found");
                return null;
            }

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            return JsonSerializer.Deserialize<RetrieveJobDetailsRespons>(json, options);
        }
    }
}
