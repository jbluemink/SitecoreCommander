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
            //string jobid = $"commander-job-{await SequenceGenerator.NextValue()}-getjob-{Guid.NewGuid():N}";
            //await SimpleLogger.Log("jobid: " + jobid);

            using HttpClient client = new();
            //client.DefaultRequestHeaders.Add("x-sc-job-id", jobid);
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token.access_token);

            using HttpResponseMessage response = await client.GetAsync(agentApiEndpoint, cancellationToken);
            string json = await response.Content.ReadAsStringAsync(cancellationToken);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            return JsonSerializer.Deserialize<List<ListJobOperationsResponse>>(json, options);
        }
    }
}
