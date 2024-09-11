using SitecoreCommander.Authoring.Model;
using SitecoreCommander.Login;
using static SitecoreCommander.Login.Request;


namespace SitecoreCommander.Authoring
{
    internal class GetItemChildren
    {

        internal static async Task<List<ResultItemWithSecurity>> GetAll(EnvironmentConfiguration env, CancellationToken cancellationToken, string itemPath)
        {
            List<ResultItemWithSecurity> result = new List<ResultItemWithSecurity>();
            var hasnext = false;
            string cursor = string.Empty;
            do
            {
                var callResult = await Get(env, cancellationToken, itemPath, cursor);
                if (callResult != null)
                {
                    hasnext = callResult.pageInfo.hasNextPage;
                    cursor = callResult.pageInfo.endCursor;
                    result.AddRange(callResult.nodes);
                } else
                {
                    hasnext = false;
                }
            } while (hasnext);

            return result;
        }

        internal static async Task<ResultItemChildrenWithSecurityChildren?> Get(EnvironmentConfiguration env, CancellationToken cancellationToken, string itemPath, string cursor)
        {
            string graphqlendpoint = env.Host;
            if (!graphqlendpoint.EndsWith("/")) { graphqlendpoint += "/"; }
            graphqlendpoint += "sitecore/api/authoring/graphql/v1/";
            string accessToken = env.AccessToken;

             Console.WriteLine("Try to get childeren from: " + itemPath);

            GraphQLResponse< ItemWithSecurityChildren> result;
            if (string.IsNullOrEmpty(cursor))
            {
                // Call GraphQL endpoint here, specifying return data type, endpoint, method, query, and variables
                result = await Request.CallGraphQLAsync<ItemWithSecurityChildren>(
                    new Uri(graphqlendpoint),
                    HttpMethod.Post,
                    accessToken,
                    "",
                    $@"query {{
  item(where: {{ path: ""{itemPath}""}}) {{
    children {{
      pageInfo {{
        hasNextPage
        endCursor
      }}
      nodes {{
        name
        itemId
        path
        security : field(name:""__Security"") {{
          value
        }}
        created:field(name  : ""__Created"") {{
            value
        }}
        access {{
          canWrite
          canDelete
          canRename
        }}
      }}
      
    }}
  }}
}}",
                    new
                    {
                        path = itemPath
                    },
                    cancellationToken);
            } else
            {
                result = await Request.CallGraphQLAsync<SitecoreCommander.Authoring.Model.ItemWithSecurityChildren>(
                    new Uri(graphqlendpoint),
                    HttpMethod.Post,
                    accessToken,
                    "",
                    $@"query {{
  item(where: {{ path: ""{itemPath}""}}) {{
    children(after: ""{cursor}"") {{
      pageInfo {{
        hasNextPage
        endCursor
      }}
      nodes {{
        name
        itemId
        path
        security : field(name:""__Security"") {{
          value
        }}
        created:field(name  : ""__Created"") {{
            value
        }}
        access {{
          canWrite
          canDelete
          canRename
        }}
      }}
      
    }}
  }}
}}",
                    new
                    {
                        path = itemPath,
                        cursor = cursor
                    },
                    cancellationToken);
            }
            // Examine the GraphQL response to see if any errors were encountered
            if (result.Errors?.Count > 0)
            {
                Console.WriteLine($"GraphQL returned errors:\n{string.Join("\n", result.Errors.Select(x => $"  - {x.Message}"))}");
                return null;
            }

            // Use the response data
            Console.WriteLine($"Childeren count fetched: {result.Data.item.children.nodes.Length} ");
            return result.Data.item.children;
        }

    }
}

