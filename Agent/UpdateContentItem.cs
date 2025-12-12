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
    internal class UpdateContentItem
    {
        internal static async Task<UpdateContentItemResponse?> UpdateItemById(JwtTokenResponse token, CancellationToken cancellationToken, string itemId, Dictionary<string, string> fields, string language, Boolean createNewVersion, string siteName, string? jobid, string? versionName = "SitecoreCommander Agent Api")
        {
            string agentApiEndpoint = "https://edge-platform.sitecorecloud.io/stream/ai-agent-api/api/v1/content/" + itemId;

            Console.WriteLine("Agent API update item: " + itemId);
            if (fields == null || fields.Count == 0)
            {
                Console.WriteLine("No fields to update.");
                return null;
            }
            using HttpClient client = new();
            if (jobid != null && !string.IsNullOrWhiteSpace(jobid))
            {
                await SimpleLogger.Log("jobid: " + jobid + " update item:"+ itemId);
                client.DefaultRequestHeaders.Add("x-sc-job-id", jobid);
            }
            if (versionName != null && !string.IsNullOrWhiteSpace(versionName))
            {
                if (fields.ContainsKey("__Version Name"))
                {
                    fields["__Version Name"] = versionName;
                }
                else
                {
                    fields.Add("__Version Name", versionName);
                }
            }

            client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token.access_token);

            string jsonBody = JsonSerializer.Serialize(new
            {
                fields,
                language,
                siteName,
                createNewVersion
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

            if (request.StatusCode != System.Net.HttpStatusCode.OK)
            {
                // 404:, return null
                Console.WriteLine("update failed:" + responseValue.Detail);
                return null;
            }

            return responseValue;
        }
    }
}
