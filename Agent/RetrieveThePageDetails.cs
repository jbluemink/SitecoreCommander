using SitecoreCommander.Agent.Model;
using SitecoreCommander.Edge.Model;
using SitecoreCommander.Login;
using SitecoreCommander.Utils;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.Json;

namespace SitecoreCommander.Agent
{
    internal class RetrieveThePageDetails
    {
        internal static async Task<RetrieveThePageDetailsResponse?> GetItemById(JwtTokenResponse token, CancellationToken cancellationToken, string pageId, string? jobid = null)
        {
            string agentApiEndpoint = "https://edge-platform.sitecorecloud.io/stream/ai-agent-api/api/v1/content/" + pageId;

            Console.WriteLine("Agent API Get page details: " + pageId);

            using HttpClient client = new();
            if (jobid != null && !string.IsNullOrWhiteSpace(jobid))
            {
                await SimpleLogger.LogAsync("jobid: " + jobid + " RetrieveThePageDetails");
                client.DefaultRequestHeaders.Add("x-sc-job-id", jobid);
            }
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token.access_token);

            using HttpResponseMessage response = await client.GetAsync(agentApiEndpoint, cancellationToken);
            string json = await response.Content.ReadAsStringAsync(cancellationToken);

            await AgentApiResponseHelper.LogTokenDiagnosticsAsync("GetPageDetails", agentApiEndpoint, token.scope);

            var responseValue = AgentApiResponseHelper.DeserializeOrThrow<RetrieveThePageDetailsResponse>(response, json, agentApiEndpoint);
            if (responseValue == null)
            {
                return null;
            }

            responseValue.__jobid = jobid ?? string.Empty;
            return responseValue;
        }
    }
}
