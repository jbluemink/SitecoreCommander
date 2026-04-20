using System.Web;
using SitecoreCommander.Login;
using SitecoreCommander.Authoring.Model;

namespace SitecoreCommander.Authoring
{ 
    //Example createpage, based on itemService item (usecase content migration)
    internal class GetItemSecurityAndDescendants
    {

        internal static async Task<SearchWithSecurity?> SearchPagination(EnvironmentConfiguration env, CancellationToken cancellationToken, string rootId, int pageSize, int page, string language)
      {
        return await SearchPagination(AuthoringApiContext.FromEnvironment(env), cancellationToken, rootId, pageSize, page, language);
      }

      internal static async Task<SearchWithSecurity?> SearchPagination(JwtTokenResponse token, string host, CancellationToken cancellationToken, string rootId, int pageSize, int page, string language)
      {
        return await SearchPagination(AuthoringApiContext.FromJwt(token, host), cancellationToken, rootId, pageSize, page, language);
      }

      internal static async Task<SearchWithSecurity?> SearchPagination(JwtContext context, CancellationToken cancellationToken, string rootId, int pageSize, int page, string language)
      {
        if (context == null)
          throw new ArgumentNullException(nameof(context));
        return await SearchPagination(AuthoringApiContext.FromJwt(
            new JwtTokenResponse { access_token = context.AccessToken }, 
            context.Host), cancellationToken, rootId, pageSize, page, language);
      }

      private static async Task<SearchWithSecurity?> SearchPagination(AuthoringApiContext context, CancellationToken cancellationToken, string rootId, int pageSize, int page, string language)
        {
            string rootidlowercase = rootId.Replace("-", "").Replace("}", "").Replace("}", "").ToLower();

            Console.WriteLine("Try to search items with autohoring api rootid" + rootidlowercase);

            // Call GraphQL endpoint here, specifying return data type, endpoint, method, query, and variables
      var result = await AuthoringGraphQl.ExecuteAsync<SearchWithSecurity>(
        context,
                 $@"query ($rootid:String! $pagesize:Int! $page:Int! $language:String!){{
  search(
    query: {{
      index: ""sitecore_master_index""
      language: $language
      searchStatement: {{
        criteria: [
          {{
            operator: MUST
            field: ""_path""
            value: $rootid
          }}
        ]
      }}
      paging: {{
        pageSize: $pagesize
        pageIndex: $page
      }}
    }}
  )
 {{
    results {{
      innerItem {{
        version
        language {{
          name
        }}
        path
        itemId
        name
        security : field(name: ""__Security"") {{
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
  totalCount
  }}
}}",
                new
                {
                    rootid = rootidlowercase,
                    pagesize = pageSize,
                    page = page,
                    language = language
                },
                cancellationToken);

            // Examine the GraphQL response to see if any errors were encountered
            if (result.Errors?.Count > 0)
            {
                Console.WriteLine($"GraphQL returned errors:\n{string.Join("\n", result.Errors.Select(x => $"  - {x.Message}"))}");
                return null;
            }

            // Use the response data
            Console.WriteLine($"Search: {result.Data.search.totalCount} items found");
            return result.Data;
         }


    }
}

