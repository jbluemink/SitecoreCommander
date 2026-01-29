using SitecoreCommander.Login;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SitecoreCommander.Authoring
{
    internal class Publish
    {
        internal static async Task<string?> PublishItem(EnvironmentConfiguration env, CancellationToken cancellationToken, string itemId, string[] languages)
        {
            return await PublishItem(env.AccessToken, env.Host, cancellationToken, itemId, languages);
        }
        internal static async Task<string?> PublishItem(JwtTokenResponse token, string host, CancellationToken cancellationToken, string itemId, string[] languages)
        {
            return await PublishItem(token.access_token, host, cancellationToken, itemId, languages);
        }

        private static async Task<string?> PublishItem(string token, string host, CancellationToken cancellationToken, string itemId, string[] languages)
        {
            string graphqlendpoint = host;
            if (!graphqlendpoint.EndsWith("/")) { graphqlendpoint += "/"; }
            graphqlendpoint += "sitecore/api/authoring/graphql/v1/";
            string accessToken = token;

            Console.WriteLine($"Try to Publish item {itemId} with languages: {string.Join(", ", languages)}");

            // Build GraphQL mutation
            string languagesList = string.Join(", ", languages.Select(l => $"\"{l}\""));
            string mutation = $@"
mutation {{
  publishItem(input: {{
    sourceDatabase: ""master""
    targetDatabases: [""experienceedge""]
    rootItemIds: [""{itemId}""]
    publishSubItems: false
    publishRelatedItems: false
    publishItemMode: FULL
    languages: [{languagesList}]
    displayName: ""Publish item Sitecore Commander""
  }}) {{
    operationId
  }}
}}";

            try
            {
                var result = await Request.CallGraphQLAsync<SitecoreCommander.Authoring.Model.PublishItemResponse>(
                    new Uri(graphqlendpoint),
                    HttpMethod.Post,
                    accessToken,
                    "",
                    mutation,
                    new { },
                    cancellationToken);

                // Examine the GraphQL response to see if any errors were encountered
                if (result.Errors?.Count > 0)
                {
                    Console.WriteLine($"GraphQL returned errors:\n{string.Join("\n", result.Errors.Select(x => $"  - {x.Message}"))}");
                     return null;
                }

                // Use the response data
                Console.WriteLine($"Publish operationId: {result.Data.publishItem.operationId}");
                return result.Data.publishItem.operationId;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in PublishItem: {ex}");
                throw;
            }
        }
    }
}
