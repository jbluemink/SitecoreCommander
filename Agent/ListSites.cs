using SitecoreCommander.Agent.Model;
using SitecoreCommander.Edge.Model;
using SitecoreCommander.Login;
using SitecoreCommander.Utils;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.Json;

namespace SitecoreCommander.Agent
{
    internal class ListSites
    {
        internal static async Task<SiteListResponse?> GetSites(JwtTokenResponse token, CancellationToken cancellationToken, string? jobid = null)
        {
            string agentApiEndpoint = "https://edge-platform.sitecorecloud.io/stream/ai-agent-api/api/v1/sites";

            Console.WriteLine("Agent API Searching for Sites  scope: " + token.scope);

            using HttpClient client = new();
            if (jobid != null && !string.IsNullOrWhiteSpace(jobid))
            {
                await SimpleLogger.LogAsync("jobid: " + jobid + " ListSites");
                client.DefaultRequestHeaders.Add("x-sc-job-id", jobid);
            }
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token.access_token);

            using HttpResponseMessage response = await client.GetAsync(agentApiEndpoint, cancellationToken);
            string json = await response.Content.ReadAsStringAsync(cancellationToken);

            await AgentApiResponseHelper.LogTokenDiagnosticsAsync("ListSites", agentApiEndpoint, token.scope);

            var responseValue = AgentApiResponseHelper.DeserializeOrThrow<SiteListResponse>(response, json, agentApiEndpoint);
            if (responseValue == null)
            {
                return null;
            }

            responseValue.__jobid = jobid ?? string.Empty;
            return responseValue;
        }
    }
}
