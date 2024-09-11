using SitecoreCommander.Authoring.Model;
using SitecoreCommander.Login;
using System.Xml.Linq;


namespace SitecoreCommander.Authoring
{
    internal class GetItemSecurity
    {
        internal static async Task<ResultItemWithSecurity?> Get(EnvironmentConfiguration env, CancellationToken cancellationToken, string itemPath)
        {
            string graphqlendpoint = env.Host;
            if (!graphqlendpoint.EndsWith("/")) { graphqlendpoint += "/"; }
            graphqlendpoint += "sitecore/api/authoring/graphql/v1/";
            string accessToken = env.AccessToken;

             Console.WriteLine("Try to get item with security fields from: " + itemPath);

            // Call GraphQL endpoint here, specifying return data type, endpoint, method, query, and variables
            var result = await Request.CallGraphQLAsync<SitecoreCommander.Authoring.Model.ItemWithSecurity>(
                new Uri(graphqlendpoint),
                HttpMethod.Post,
                accessToken,
                "",
                $@"query ($path:String) {{
  item(where: {{ path: $path }}) {{
    name
    itemId
    path
    version
    language {{
        name
    }}
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
}}",
                new
                {
                    path = itemPath
                },
                cancellationToken);

            // Examine the GraphQL response to see if any errors were encountered
            if (result.Errors?.Count > 0)
            {
                Console.WriteLine($"GraphQL returned errors:\n{string.Join("\n", result.Errors.Select(x => $"  - {x.Message}"))}");
                return null;
            }

            // Use the response data
            Console.WriteLine($"Item fetched with Id: {result.Data.item.itemId} ");
            return result.Data.item;
        }

    }
}

