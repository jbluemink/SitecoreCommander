using SitecoreCommander.Agent.Model;
using SitecoreCommander.Edge.Model;
using SitecoreCommander.Login;
using SitecoreCommander.Utils;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace SitecoreCommander.Agent
{
    internal class UpdateContentItem
    {
        internal static async Task<UpdateContentItemResponse> GetItemById(JwtTokenResponse token, CancellationToken cancellationToken, string itemId, Dictionary<string, string> fields, string language, Boolean createNewVersion, string siteName)
        {
            string agentApiEndpoint = "https://edge-platform.sitecorecloud.io/stream/ai-agent-api/api/v1/content/" + itemId;

            Console.WriteLine("Agent API update page: " + itemId);
            string jobid = $"commander-job-{await SequenceGenerator.NextValue()}-updateitem-{Guid.NewGuid():N}";
            await SimpleLogger.Log("jobid: " + jobid);

            using HttpClient client = new();
            client.DefaultRequestHeaders.Add("x-sc-job-id", jobid);
            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token.access_token);

            string jsonBody = JsonSerializer.Serialize(new
            {
                fields = fields,
                language = language,
                createNewVersion,
                siteName = siteName
            });
            using StringContent postData = new(jsonBody, Encoding.UTF8, "application/json");
            using HttpResponseMessage request = await client.PutAsync(agentApiEndpoint, postData);
            string json = await request.Content.ReadAsStringAsync();


            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var responseValue = JsonSerializer.Deserialize<UpdateContentItemResponse>(json, options);
            responseValue!.__jobid = jobid;
            return responseValue;
        }
    }
}
