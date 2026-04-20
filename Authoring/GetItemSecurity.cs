using SitecoreCommander.Authoring.Model;
using SitecoreCommander.Login;
using System.Xml.Linq;


namespace SitecoreCommander.Authoring
{
    public class GetItemSecurity
    {
        public static async Task<ResultItemWithSecurity?> Get(EnvironmentConfiguration env, CancellationToken cancellationToken, string itemPath)
        {
            return await Get(AuthoringApiContext.FromEnvironment(env), cancellationToken, itemPath);
        }

        internal static async Task<ResultItemWithSecurity?> Get(JwtTokenResponse token, string host, CancellationToken cancellationToken, string itemPath)
        {
            return await Get(AuthoringApiContext.FromJwt(token, host), cancellationToken, itemPath);
        }

        internal static async Task<ResultItemWithSecurity?> Get(JwtContext context, CancellationToken cancellationToken, string itemPath)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            return await Get(AuthoringApiContext.FromJwt(
                new JwtTokenResponse { access_token = context.AccessToken }, 
                context.Host), cancellationToken, itemPath);
        }

        private static async Task<ResultItemWithSecurity?> Get(AuthoringApiContext context, CancellationToken cancellationToken, string itemPath)
        {
             Console.WriteLine("Try to get item with security fields from: " + itemPath);

            // Call GraphQL endpoint here, specifying return data type, endpoint, method, query, and variables
            var result = await AuthoringGraphQl.ExecuteAsync<SitecoreCommander.Authoring.Model.ItemWithSecurity>(
                context,
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
            if (result.Data.item == null)
            {
                Console.WriteLine($"item not found ,");
                return null;
            } 

            if (result.Data == null || result.Data.item == null)
            {
                return null;
            }

                // Use the response data
                Console.WriteLine($"Item fetched with Id: {result.Data.item.itemId} ");
            return result.Data.item;
        }

    }
}

