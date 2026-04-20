using SitecoreCommander.Authoring.Model;
using SitecoreCommander.Login;
using static SitecoreCommander.Login.Request;


namespace SitecoreCommander.Authoring
{
    internal class GetItemChildren
    {

        internal static async Task<List<ResultItemWithSecurity>> GetAll(EnvironmentConfiguration env, CancellationToken cancellationToken, string itemPath)
      {
        return await GetAll(AuthoringApiContext.FromEnvironment(env), cancellationToken, itemPath);
      }

      internal static async Task<List<ResultItemWithSecurity>> GetAll(JwtTokenResponse token, string host, CancellationToken cancellationToken, string itemPath)
      {
        return await GetAll(AuthoringApiContext.FromJwt(token, host), cancellationToken, itemPath);
      }

      internal static async Task<List<ResultItemWithSecurity>> GetAll(JwtContext context, CancellationToken cancellationToken, string itemPath)
      {
        if (context == null)
          throw new ArgumentNullException(nameof(context));
        return await GetAll(AuthoringApiContext.FromJwt(
            new JwtTokenResponse { access_token = context.AccessToken }, 
            context.Host), cancellationToken, itemPath);
      }

      private static async Task<List<ResultItemWithSecurity>> GetAll(AuthoringApiContext context, CancellationToken cancellationToken, string itemPath)
        {
            List<ResultItemWithSecurity> result = new List<ResultItemWithSecurity>();
            var hasnext = false;
            string cursor = string.Empty;
            do
            {
          var callResult = await Get(context, cancellationToken, itemPath, cursor);
                if (callResult != null)
                {
              hasnext = callResult.pageInfo?.hasNextPage ?? false;
              cursor = callResult.pageInfo?.endCursor ?? string.Empty;
              if (callResult.nodes != null && callResult.nodes.Length > 0)
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
            return await Get(AuthoringApiContext.FromEnvironment(env), cancellationToken, itemPath, cursor);
          }

          internal static async Task<ResultItemChildrenWithSecurityChildren?> Get(JwtTokenResponse token, string host, CancellationToken cancellationToken, string itemPath, string cursor)
          {
            return await Get(AuthoringApiContext.FromJwt(token, host), cancellationToken, itemPath, cursor);
          }

          internal static async Task<ResultItemChildrenWithSecurityChildren?> Get(JwtContext context, CancellationToken cancellationToken, string itemPath, string cursor)
          {
            if (context == null)
              throw new ArgumentNullException(nameof(context));
            return await Get(AuthoringApiContext.FromJwt(
                new JwtTokenResponse { access_token = context.AccessToken }, 
                context.Host), cancellationToken, itemPath, cursor);
          }

          private static async Task<ResultItemChildrenWithSecurityChildren?> Get(AuthoringApiContext context, CancellationToken cancellationToken, string itemPath, string cursor)
        {
             Console.WriteLine("Try to get childeren from: " + itemPath);

            GraphQLResponse< ItemWithSecurityChildren> result;
            if (string.IsNullOrEmpty(cursor))
            {
                // Call GraphQL endpoint here, specifying return data type, endpoint, method, query, and variables
              result = await AuthoringGraphQl.ExecuteAsync<ItemWithSecurityChildren>(
                context,
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
                  result = await AuthoringGraphQl.ExecuteAsync<SitecoreCommander.Authoring.Model.ItemWithSecurityChildren>(
                    context,
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
            if (result?.Errors?.Count > 0)
            {
                Console.WriteLine($"GraphQL returned errors:\n{string.Join("\n", result.Errors.Select(x => $"  - {x.Message}"))}");
                return null;
            }

            if (result?.Data?.item?.children == null)
            {
              Console.WriteLine($"No children payload returned for item path: {itemPath}");
              return new ResultItemChildrenWithSecurityChildren
              {
                pageInfo = new PageInfo { hasNextPage = false, endCursor = string.Empty },
                nodes = Array.Empty<ResultItemWithSecurity>()
              };
            }

            // Use the response data
            Console.WriteLine($"Childeren count fetched: {result.Data.item.children.nodes?.Length ?? 0} ");
            return result.Data.item.children;
        }

    }
}

