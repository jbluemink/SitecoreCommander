using System.Web;
using SitecoreCommander.Login;
using SitecoreCommander.Authoring.Model;

namespace SitecoreCommander.Authoring
{ 
  //Example createpage, based on itemService item (usecase content migration)  
    internal class GetItemFieldAndDescendants
    {

        internal static async Task<SearchWithFieldResult> SearchPagination(EnvironmentConfiguration env, CancellationToken cancellationToken, string rootId, string fieldname, int pageSize, int page, string language, string? templateName = null)
        {
            string graphqlendpoint = env.Host;
            if (!graphqlendpoint.EndsWith("/")) { graphqlendpoint += "/"; }
            graphqlendpoint += "sitecore/api/authoring/graphql/v1/";
            string accessToken = env.AccessToken;
            string rootidlowercase = rootId.Replace("-", "").Replace("}", "").Replace("}", "").ToLower();

            Console.WriteLine("Try to search items with authoring api rootid" + rootidlowercase);
            var criteria = new List<SearchCriteriaInput>
            {
                new SearchCriteriaInput { Field = "_path", Value = rootidlowercase, Operator = CriteriaOperator.MUST }
            };
            if (!string.IsNullOrEmpty(templateName))
            {
                criteria.Add(new SearchCriteriaInput { Field = "_templatename", Value = templateName, Operator = CriteriaOperator.MUST, CriteriaType = CriteriaType.EXACT });
            }

            // Call GraphQL endpoint here, specifying return data type, endpoint, method, query, and variables  
            var result = await Request.CallGraphQLAsync<SearchWithFieldResult>(
                new Uri(graphqlendpoint),
                HttpMethod.Post,
                accessToken,
                "",
                 $@"query ($criteria: [SearchCriteriaInput!]!, $pagesize:Int! $page:Int! $language:String!){{  
     search(  
       query: {{  
         index: ""sitecore_master_index""  
         language: $language  
         searchStatement: {{  
           criteria: $criteria
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
           name
           itemId
           fieldvalue: field(name: ""{fieldname}"") {{  
             value  
           }}
         }}  
       }}  
     totalCount  
     }}  
    }}",
                new
                {
                    criteria = criteria,
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

