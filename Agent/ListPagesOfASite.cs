using SitecoreCommander.Agent.Model;
using SitecoreCommander.Edge.Model;
using SitecoreCommander.Login;
using SitecoreCommander.Utils;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.Json;

namespace SitecoreCommander.Agent
{
    internal class ListSitListPagesOfASitees
    {
        internal static async Task<ListPagesOfASiteResponse?> GetPages(JwtTokenResponse token, CancellationToken cancellationToken, string siteName, string language, string? jobid = null)
        {
            string agentApiEndpoint = "https://edge-platform.sitecorecloud.io/stream/ai-agent-api/api/v1/sites/"+siteName+"/pages?language="+language;

            Console.WriteLine("Agent API Searching for Pages sitename: " + siteName);

            using HttpClient client = new();
            if (jobid != null && !string.IsNullOrWhiteSpace(jobid))
            {
                await SimpleLogger.LogAsync("jobid: " + jobid + " listpages");
                client.DefaultRequestHeaders.Add("x-sc-job-id", jobid);
            }
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token.access_token);

            using HttpResponseMessage response = await client.GetAsync(agentApiEndpoint, cancellationToken);
            string json = await response.Content.ReadAsStringAsync(cancellationToken);

            await AgentApiResponseHelper.LogTokenDiagnosticsAsync("ListPages", agentApiEndpoint, token.scope);

            List<ListPagesItem>? items = AgentApiResponseHelper.DeserializeOrThrow<List<ListPagesItem>>(response, json, agentApiEndpoint)
                ?? new List<ListPagesItem>();

            var responseValue = new ListPagesOfASiteResponse
            {
                Items = items,
                __jobid = jobid ?? string.Empty
            };
            return responseValue;
        }
    }
}
