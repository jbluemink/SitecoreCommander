using SitecoreCommander.Edge.Model;
using SitecoreCommander.Login;
using SitecoreCommander.Edge;

namespace SitecoreCommander.Edge
{
    internal class GetItemVersionsAndDescendants
    {
        /// <summary>
        /// get item by path or id
        /// </summary>
        internal static async Task<SearchPaginationItems> SearchMax100Results(EnvironmentConfiguration env, CancellationToken cancellationToken, string itemid)
        {
            return await SearchPagination(env, cancellationToken, itemid, Config.DefaultLanguage, "");
        }

        internal static async Task<SearchPaginationItems> SearchPagination(EnvironmentConfiguration env, CancellationToken cancellationToken, string itemid, string language, string endCursor)
        {
            string graphqlendpoint = env.Host;
            if (!graphqlendpoint.EndsWith("/")) { graphqlendpoint += "/"; }
            graphqlendpoint += "sitecore/api/graph/edge/";
            string apikey = Config.apikey;//userJson.endpoints.@default.accessToken;

            Console.WriteLine("Searching for Sitecore Item and Descendants: guid=" + itemid);

            // Call GraphQL endpoint here, specifying return data type, endpoint, method, query, and variables
            
             var result = await Request.CallGraphQLAsync<SearchPaginationItems>(
                new Uri(graphqlendpoint),
                HttpMethod.Post,
                "",
                apikey,
                $@"
query {{
  pageOne: search(
     where: {{
       AND: [
         {{
           name: ""_path""
           value: ""{itemid}""
           operator: CONTAINS
         }}
         {{
           name: ""_language""
           value: ""{language}""
           operator: EQ
         }}
       ]
     }}
     first: 100
     " + EdgeHelper.QueryFormatRemoveIfEmpty("after", endCursor)  + @"
   ) {
     total
     pageInfo {
       endCursor
       hasNext
     }
     results {
      id
      path
      version
      language {
        name
      }
     }
   }
 }",
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
            if (result.Data.pageOne == null)
            {
                Console.WriteLine($"nothings found ,");
            }
            else
            {
                Console.WriteLine($"Found {result.Data.pageOne.results.Length} item versions of a total {result.Data.pageOne.total} ,");
            }
            return result.Data;
        }

    }

}

