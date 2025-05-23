using SitecoreCommander.Authoring.Model;
using SitecoreCommander.Login;

namespace SitecoreCommander.Authoring
{
    internal class DeleteItem
    {
        internal static async Task<bool> Delete(EnvironmentConfiguration env, CancellationToken cancellationToken, string itemPath)
        {
            string graphqlendpoint = env.Host;
            if (!graphqlendpoint.EndsWith("/")) { graphqlendpoint += "/"; }
            graphqlendpoint += "sitecore/api/authoring/graphql/v1/";
            string accessToken = env.AccessToken;

             Console.WriteLine("Try to Delete item " + itemPath);

            // Call GraphQL endpoint here, specifying return data type, endpoint, method, query, and variables
            var result = await Request.CallGraphQLAsync<SitecoreCommander.Authoring.Model.DeleteItemResponse>(
                new Uri(graphqlendpoint),
                HttpMethod.Post,
                accessToken,
                "",
                "mutation DeleteItem {" +
                "deleteItem(" +
                "input: {" +
                "\r\n path: \"" + itemPath + "\"" +
                "\r\n permanently: false" +
               "\r\n}\r\n  ) {\r\n successful }\r\n}",
                new
                {
                },
                cancellationToken, TimeSpan.FromMinutes(10));

            // Examine the GraphQL response to see if any errors were encountered
            if (result.Errors?.Count > 0)
            {
                Console.WriteLine($"GraphQL returned errors:\n{string.Join("\n", result.Errors.Select(x => $"  - {x.Message}"))}");
                return false;
            }

            // Use the response data
            Console.WriteLine($"Item deleted Id: {result.Data.deleteItem.successful} ");
            return result.Data.deleteItem.successful;
        }

    }
}

