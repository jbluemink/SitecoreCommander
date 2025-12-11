using SitecoreCommander.Agent.Model;
using SitecoreCommander.Authoring.Model;
using SitecoreCommander.Edge.Model;
using SitecoreCommander.Login;
using SitecoreCommander.Utils;
using System.Net.Http;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace SitecoreCommander.Agent
{
    internal class DeleteContentItem
    {
        internal static async Task<DeleteContentItemResponse?> DeleteItemById(JwtTokenResponse token, CancellationToken cancellationToken, string itemId,  string? language, string? jobid)
        {
            string agentApiEndpoint = "https://edge-platform.sitecorecloud.io/stream/ai-agent-api/api/v1/content/" + itemId;
            if (language != null && !string.IsNullOrWhiteSpace(language))
            {
                agentApiEndpoint += "?language=" + language;
            }
            Console.WriteLine("Agent API delete item: " + itemId);
            using HttpClient client = new();
            if (jobid != null && !string.IsNullOrWhiteSpace(jobid))
            {
                await SimpleLogger.Log("jobid: " + jobid + " delete item:"+ itemId);
                client.DefaultRequestHeaders.Add("x-sc-job-id", jobid);
            }

            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token.access_token);

            using HttpResponseMessage request = await client.DeleteAsync(agentApiEndpoint);
            string json = await request.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var responseValue = JsonSerializer.Deserialize<DeleteContentItemResponse>(json, options);
            if (responseValue == null)
            {
                return null;
            }

            responseValue.__jobid = jobid;

            if (request.StatusCode != System.Net.HttpStatusCode.OK)
            {
                // 404:, return null
                Console.WriteLine("update faild:" + responseValue.Detail);
                return null;
            }

            return responseValue;
        }
    }
}
