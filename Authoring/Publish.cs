using SitecoreCommander.Login;

namespace SitecoreCommander.Authoring
{
    internal class Publish
    {
        internal static async Task<string?> PublishItem(EnvironmentConfiguration env, CancellationToken cancellationToken, string itemId, string[] languages)
        {
            return await PublishItem(AuthoringApiContext.FromEnvironment(env), cancellationToken, itemId, languages);
        }

        internal static async Task<string?> PublishItem(JwtTokenResponse token, string host, CancellationToken cancellationToken, string itemId, string[] languages)
        {
            return await PublishItem(AuthoringApiContext.FromJwt(token, host), cancellationToken, itemId, languages);
        }

        internal static async Task<string?> PublishItem(JwtContext context, CancellationToken cancellationToken, string itemId, string[] languages)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            return await PublishItem(AuthoringApiContext.FromJwt(
                new JwtTokenResponse { access_token = context.AccessToken }, 
                context.Host), cancellationToken, itemId, languages);
        }

        private static async Task<string?> PublishItem(AuthoringApiContext context, CancellationToken cancellationToken, string itemId, string[] languages)
        {
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
                var result = await AuthoringGraphQl.ExecuteAsync<SitecoreCommander.Authoring.Model.PublishItemResponse>(
                    context,
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
