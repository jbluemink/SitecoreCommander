using SitecoreCommander.Agent.Model;
using SitecoreCommander.Edge.Model;
using SitecoreCommander.Login;
using SitecoreCommander.Utils;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.Json;

namespace SitecoreCommander.Agent
{
    internal class RetrieveContentItemById
    {
        internal static async Task<RetrieveThePageDetailsResponse?> GetItemById(JwtTokenResponse token, CancellationToken cancellationToken, string itemId, string language, string? jobid = null)
        {
            string agentApiEndpoint = "https://edge-platform.sitecorecloud.io/stream/ai-agent-api/api/v1/content/" + itemId;
            if (language != null && !string.IsNullOrWhiteSpace(language))
            {
                agentApiEndpoint += "?language=" + language;
            }
            Console.WriteLine("Agent API Get item: " + itemId);

            using HttpClient client = new();
            if (jobid != null && !string.IsNullOrWhiteSpace(jobid))
            {
                await SimpleLogger.Log("jobid: " + jobid + " RetrieveThePageDetails");
                client.DefaultRequestHeaders.Add("x-sc-job-id", jobid);
            }
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token.access_token);

            using HttpResponseMessage response = await client.GetAsync(agentApiEndpoint, cancellationToken);
            string json = await response.Content.ReadAsStringAsync(cancellationToken);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var responseValue = JsonSerializer.Deserialize<RetrieveThePageDetailsResponse>(json, options);
            responseValue!.__jobid = jobid;
            return responseValue;
        }
    }
}
