using SitecoreCommander.Authoring.Model;
using SitecoreCommander.Login;

namespace SitecoreCommander.Authoring
{
    internal class AddItemVersion
    {
        internal static async Task<ResultItem?> Add(EnvironmentConfiguration env, CancellationToken cancellationToken, string itemId, string language)
        {
            return await Add(AuthoringApiContext.FromEnvironment(env), cancellationToken, itemId, language);
        }

        internal static async Task<ResultItem?> Add(JwtTokenResponse token, string host, CancellationToken cancellationToken, string itemId, string language)
        {
            return await Add(AuthoringApiContext.FromJwt(token, host), cancellationToken, itemId, language);
        }

        internal static async Task<ResultItem?> Add(JwtContext context, CancellationToken cancellationToken, string itemId, string language)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            return await Add(AuthoringApiContext.FromJwt(
                new JwtTokenResponse { access_token = context.AccessToken }, 
                context.Host), cancellationToken, itemId, language);
        }

        internal static async Task<ResultItem?> Add(AuthoringApiContext context, CancellationToken cancellationToken, string itemId, string language)
        {

            Console.WriteLine("Try to Add version item " + itemId);

            // Call GraphQL endpoint here, specifying return data type, endpoint, method, query, and variables
            var result = await AuthoringGraphQl.ExecuteAsync<SitecoreCommander.Authoring.Model.AddItemVersion>(
                context,
                "mutation addItemVersion{" +
                "addItemVersion(" +
                "input: {" +
                "\r\n itemId: \"" + itemId + "\"" +
                "\r\n language: \"" + language + "\"" +
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
            Console.WriteLine($"Item version created with Id: {result.Data.addItemVersion.item.itemId} ");
            return result.Data.addItemVersion.item;
        }

    }
}

