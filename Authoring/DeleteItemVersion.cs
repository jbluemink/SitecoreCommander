using SitecoreCommander.Authoring.Model;
using SitecoreCommander.Edge;
using SitecoreCommander.Login;

namespace SitecoreCommander.Authoring
{
    internal class DeleteItemVersion
    {
        internal static async Task<ResultItem?> DeleteAll(EnvironmentConfiguration env, CancellationToken cancellationToken, string itemId, string language)
        {
            ResultItem? result;
            do
            {
                result = await Delete(env, cancellationToken, itemId, language, "");
            } while (result != null && !cancellationToken.IsCancellationRequested);

            return result;
        }
        internal static async Task<ResultItem?> Delete(EnvironmentConfiguration env, CancellationToken cancellationToken, string itemId, string language, string version)
        {
            string graphqlendpoint = env.Host;
            if (!graphqlendpoint.EndsWith("/")) { graphqlendpoint += "/"; }
            graphqlendpoint += "sitecore/api/authoring/graphql/v1/";
            string accessToken = env.AccessToken;

            Console.WriteLine($@"Try to Delete version {version} {language} from item {itemId}");

            // Call GraphQL endpoint here, specifying return data type, endpoint, method, query, and variables
            var result = await Request.CallGraphQLAsync<SitecoreCommander.Authoring.Model.DeleteItemVersion>(
                new Uri(graphqlendpoint),
                HttpMethod.Post,
                accessToken,
                "",
                $@"mutation deleteItemVersion{{
deleteItemVersion(
 input: {{
   itemId: ""{itemId}"" 
   language: ""{language}""
   {EdgeHelper.QueryFormatIntRemoveIfEmpty("version",version)}
}}
) {{
    item {{
      itemId
    }}
  }}
}}",
                new
                {
                },
                cancellationToken);

            // Examine the GraphQL response to see if any errors were encountered
            if (result.Errors?.Count > 0)
            {
                Console.WriteLine($"GraphQL returned errors:\n{string.Join("\n", result.Errors.Select(x => $"  - {x.Message}"))}");
                return null;
            }

            // Use the response data
            Console.WriteLine($"Item version deleted");
            return result.Data.deleteItemVersion.item;
        }


    }
}

