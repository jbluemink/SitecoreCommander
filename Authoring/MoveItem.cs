using SitecoreCommander.Authoring.Model;
using SitecoreCommander.Login;

namespace SitecoreCommander.Authoring
{
    internal class MoveItem
    {
        internal static async Task<ResultItem?> Move(EnvironmentConfiguration env, CancellationToken cancellationToken, string itemPath, string targetParentPath)
        {
            return await Move(AuthoringApiContext.FromEnvironment(env), cancellationToken, itemPath, targetParentPath);
        }

        internal static async Task<ResultItem?> Move(JwtTokenResponse token, string host, CancellationToken cancellationToken, string itemPath, string targetParentPath)
        {
            return await Move(AuthoringApiContext.FromJwt(token, host), cancellationToken, itemPath, targetParentPath);
        }

        private static async Task<ResultItem?> Move(AuthoringApiContext context, CancellationToken cancellationToken, string itemPath, string targetParentPath)
        {

             Console.WriteLine("Try to Move item " + itemPath);

            // Call GraphQL endpoint here, specifying return data type, endpoint, method, query, and variables
            var result = await AuthoringGraphQl.ExecuteAsync<SitecoreCommander.Authoring.Model.MoveItem>(
                context,
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
            return await Move(AuthoringApiContext.FromEnvironment(env), cancellationToken, itemId, targetParentId);
        }

        internal static async Task<ResultItem?> Move(JwtTokenResponse token, string host, CancellationToken cancellationToken, Guid itemId, Guid targetParentId)
        {
            return await Move(AuthoringApiContext.FromJwt(token, host), cancellationToken, itemId, targetParentId);
        }

        private static async Task<ResultItem?> Move(AuthoringApiContext context, CancellationToken cancellationToken, Guid itemId, Guid targetParentId)
        {

            Console.WriteLine("Try to Move item " + itemId);

            // Call GraphQL endpoint here, specifying return data type, endpoint, method, query, and variables
            var result = await AuthoringGraphQl.ExecuteAsync<SitecoreCommander.Authoring.Model.MoveItem>(
                context,
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

