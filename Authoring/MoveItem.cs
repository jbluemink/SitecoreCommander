using SitecoreCommander.Authoring.Model;
using SitecoreCommander.Login;

namespace SitecoreCommander.Authoring
{
    internal class MoveItem
    {
        internal static async Task<ResultItem?> Move(EnvironmentConfiguration env, CancellationToken cancellationToken, string itemPath, string targetParentPath)
        {
            string graphqlendpoint = env.Host;
            if (!graphqlendpoint.EndsWith("/")) { graphqlendpoint += "/"; }
            graphqlendpoint += "sitecore/api/authoring/graphql/v1/";
            string accessToken = env.AccessToken;

             Console.WriteLine("Try to Move item " + itemPath);

            // Call GraphQL endpoint here, specifying return data type, endpoint, method, query, and variables
            var result = await Request.CallGraphQLAsync<SitecoreCommander.Authoring.Model.MoveItem>(
                new Uri(graphqlendpoint),
                HttpMethod.Post,
                accessToken,
                "",
                "mutation MoveItem {" +
                "moveItem(" +
                "input: {" +
                "\r\n path: \"" + itemPath + "\"" +
                "\r\n targetParentPath: \"" + targetParentPath + "\"" +
               "\r\n}\r\n  ) {\r\n    item {\r\n      itemId\r\n    }\r\n  }\r\n}",
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
            Console.WriteLine($"Item moved Id: {result.Data.moveItem.item.itemId} ");
            return result.Data.moveItem.item;
        }


        internal static async Task<ResultItem?> Move(EnvironmentConfiguration env, CancellationToken cancellationToken, Guid itemId, Guid targetParentId)
        {
            string graphqlendpoint = env.Host;
            if (!graphqlendpoint.EndsWith("/")) { graphqlendpoint += "/"; }
            graphqlendpoint += "sitecore/api/authoring/graphql/v1/";
            string accessToken = env.AccessToken;

            Console.WriteLine("Try to Move item " + itemId);

            // Call GraphQL endpoint here, specifying return data type, endpoint, method, query, and variables
            var result = await Request.CallGraphQLAsync<SitecoreCommander.Authoring.Model.MoveItem>(
                new Uri(graphqlendpoint),
                HttpMethod.Post,
                accessToken,
                "",
                "mutation MoveItem {" +
                "moveItem(" +
                "input: {" +
                "\r\n itemId: \"" + itemId + "\"" +
                "\r\n targetParentId: \"" + targetParentId + "\"" +
               "\r\n}\r\n  ) {\r\n    item {\r\n      itemId\r\n    }\r\n  }\r\n}",
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
            Console.WriteLine($"Item version created with Id: {result.Data.moveItem.item.itemId} ");
            return result.Data.moveItem.item;
        }

    }
}

