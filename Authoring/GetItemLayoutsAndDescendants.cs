using SitecoreCommander.Authoring.Model;
using SitecoreCommander.Login;

namespace SitecoreCommander.Authoring
{
    internal class GetItemLayoutsAndDescendants
    {
        internal static async Task<SearchWithLayoutsResult?> SearchPagination(
            EnvironmentConfiguration env,
            CancellationToken cancellationToken,
            string rootId,
            int pageSize,
            int page,
            string language,
            string? templateName = null)
          {
            return await SearchPagination(AuthoringApiContext.FromEnvironment(env), cancellationToken, rootId, pageSize, page, language, templateName);
          }

          internal static async Task<SearchWithLayoutsResult?> SearchPagination(
            JwtTokenResponse token,
            string host,
            CancellationToken cancellationToken,
            string rootId,
            int pageSize,
            int page,
            string language,
            string? templateName = null)
          {
            return await SearchPagination(AuthoringApiContext.FromJwt(token, host), cancellationToken, rootId, pageSize, page, language, templateName);
          }

          internal static async Task<SearchWithLayoutsResult?> SearchPagination(
            JwtContext context,
            CancellationToken cancellationToken,
            string rootId,
            int pageSize,
            int page,
            string language,
            string? templateName = null)
          {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            return await SearchPagination(AuthoringApiContext.FromJwt(
                new JwtTokenResponse { access_token = context.AccessToken }, 
                context.Host), cancellationToken, rootId, pageSize, page, language, templateName);
          }

          private static async Task<SearchWithLayoutsResult?> SearchPagination(
            AuthoringApiContext context,
            CancellationToken cancellationToken,
            string rootId,
            int pageSize,
            int page,
            string language,
            string? templateName = null)
        {
            string rootidlowercase = rootId.Replace("-", "").Replace("{", "").Replace("}", "").ToLower();

            Console.WriteLine("Try to search item layouts with authoring api rootid " + rootidlowercase);

            var criteria = new List<SearchCriteriaInput>
            {
                new SearchCriteriaInput { Field = "_path", Value = rootidlowercase, Operator = CriteriaOperator.MUST }
            };
            if (!string.IsNullOrEmpty(templateName))
            {
                criteria.Add(new SearchCriteriaInput { Field = "_templatename", Value = templateName, Operator = CriteriaOperator.MUST, CriteriaType = CriteriaType.EXACT });
            }

            var result = await AuthoringGraphQl.ExecuteAsync<SearchWithLayoutsResult>(
              context,
                @"query ($criteria: [SearchCriteriaInput!]!, $pagesize:Int!, $page:Int!, $language:String!) {
  search(
    query: {
      index: ""sitecore_master_index""
      language: $language
      searchStatement: {
        criteria: $criteria
      }
      paging: {
        pageSize: $pagesize
        pageIndex: $page
      }
    }
  ) {
    results {
      innerItem {
        version
        language {
          name
        }
        path
        name
        itemId
        template {
          templateId
          name
        }
        sharedLayout: field(name: ""__Renderings"") {
          value
        }
        finalLayout: field(name: ""__Final Renderings"") {
          value
        }
      }
    }
    totalCount
  }
}",
                new
                {
                    criteria = criteria,
                    pagesize = pageSize,
                    page = page,
                    language = language
                },
                cancellationToken);

            if (result.Errors?.Count > 0)
            {
                Console.WriteLine($"GraphQL returned errors:\n{string.Join("\n", result.Errors.Select(x => $"  - {x.Message}"))}");
                return null;
            }

            Console.WriteLine($"Search layouts: {result.Data.search.totalCount} items found");
            return result.Data;
        }
    }
}
