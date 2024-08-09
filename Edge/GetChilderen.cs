using SitecoreCommander.Edge.Model;
using SitecoreCommander.Login;

namespace SitecoreCommander.Edge
{
    internal class GetChilderen
    {
        /// <summary>
        /// get item by path or id
        /// </summary>
        internal static async Task<ResultGetItem[]> Get(EnvironmentConfiguration env, CancellationToken cancellationToken, string itemPath)
        {
            return await Get(env, cancellationToken, itemPath, Config.DefaultLanguage);
        }

        internal static async Task<ResultGetItem[]> Get(EnvironmentConfiguration env, CancellationToken cancellationToken, string itemPath, string language)
        {
            string graphqlendpoint = env.Host;
            if (!graphqlendpoint.EndsWith("/")) { graphqlendpoint += "/"; }
            graphqlendpoint += "sitecore/api/graph/edge/";
            string apikey = Config.apikey;//userJson.endpoints.@default.accessToken;

            Console.WriteLine("Searching for Sitecore Item:" + itemPath);

            // Call GraphQL endpoint here, specifying return data type, endpoint, method, query, and variables

            var result = await Request.CallGraphQLAsync<ResultGetChilderen>(
               new Uri(graphqlendpoint),
               HttpMethod.Post,
               "",
               apikey,
               $@"query {{
  item(path: ""{itemPath}"", language: ""{language}"") {{
    children {{
      results {{
      name
        id
        path
      }}
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
            if (result.Data.item == null)
            {
                Console.WriteLine($"item not found ,");
                return null;
            }
            else
            {
                Console.WriteLine($"Found  {result.Data.item.children.results.Length} childeren");
                return result.Data.item.children.results;
            }

        }

    }
}

