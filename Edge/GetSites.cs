using SitecoreCommander.Edge.Model;
using SitecoreCommander.Login;

namespace SitecoreCommander.Edge
{
    internal class GetEdgeSites
    {

        internal static async Task<Site[]> Get(EnvironmentConfiguration env, CancellationToken cancellationToken)
        {
            string graphqlendpoint = env.Host;
            if (!graphqlendpoint.EndsWith("/")) { graphqlendpoint += "/"; }
            graphqlendpoint += "sitecore/api/graph/edge/";
            string apikey = Config.apikey;

            Console.WriteLine("Searching for Sites with Edge API");

            // Call GraphQL endpoint here, specifying return data type, endpoint, method, query, and variables

            var result = await Request.CallGraphQLAsync<ResultGetSites>(
               new Uri(graphqlendpoint),
               HttpMethod.Post,
               "",
               apikey,
               $@"query {{
    site {{
        siteInfoCollection {{
            hostname
            language
            name
            rootPath
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
            if (result.Data.site == null)
            {
                Console.WriteLine($"no sites found,");
                return null;
            }
            else
            {
                Console.WriteLine($"Found  {result.Data.site.siteInfoCollection.Length} sites");
                return result.Data.site.siteInfoCollection;
            }

        }

    }
}

