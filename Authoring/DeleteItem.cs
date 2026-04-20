using SitecoreCommander.Authoring.Model;
using SitecoreCommander.Login;

namespace SitecoreCommander.Authoring
{
    internal class DeleteItem
    {
        internal static async Task<bool> Delete(EnvironmentConfiguration env, CancellationToken cancellationToken, string itemPath)
        {
            return await Delete(AuthoringApiContext.FromEnvironment(env), cancellationToken, itemPath);
        }

        internal static async Task<bool> Delete(JwtTokenResponse token, string host, CancellationToken cancellationToken, string itemPath)
        {
            return await Delete(AuthoringApiContext.FromJwt(token, host), cancellationToken, itemPath);
        }

        internal static async Task<bool> Delete(JwtContext context, CancellationToken cancellationToken, string itemPath)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            return await Delete(AuthoringApiContext.FromJwt(
                new JwtTokenResponse { access_token = context.AccessToken }, 
                context.Host), cancellationToken, itemPath);
        }

        private static async Task<bool> Delete(AuthoringApiContext context, CancellationToken cancellationToken, string itemPath)
        {

             Console.WriteLine("Try to Delete item " + itemPath);

            // Call GraphQL endpoint here, specifying return data type, endpoint, method, query, and variables
            var result = await Request.CallGraphQLAsync<SitecoreCommander.Authoring.Model.DeleteItemResponse>(
                context.GraphQlEndpoint,
                HttpMethod.Post,
                context.AccessToken,
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

