using System.Web;
using SitecoreCommander.Login;
using SitecoreCommander.Authoring.Model;

namespace SitecoreCommander.Authoring
{ 
    //Example createpage, based on itemService item (usecase content migration)
    internal class UpdateItemVersionUnpublish
    {

        internal static async Task<Created?> UpdateItem(EnvironmentConfiguration env, CancellationToken cancellationToken, string itemId, int version, string hideVersion, string language)
        {
            return await UpdateItem(AuthoringApiContext.FromEnvironment(env), cancellationToken, itemId, version, hideVersion, language);
        }

        internal static async Task<Created?> UpdateItem(JwtTokenResponse token, string host, CancellationToken cancellationToken, string itemId, int version, string hideVersion, string language)
        {
            return await UpdateItem(AuthoringApiContext.FromJwt(token, host), cancellationToken, itemId, version, hideVersion, language);
        }

        internal static async Task<Created?> UpdateItem(JwtContext context, CancellationToken cancellationToken, string itemId, int version, string hideVersion, string language)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            return await UpdateItem(AuthoringApiContext.FromJwt(
                new JwtTokenResponse { access_token = context.AccessToken }, 
                context.Host), cancellationToken, itemId, version, hideVersion, language);
        }

        private static async Task<Created?> UpdateItem(AuthoringApiContext context, CancellationToken cancellationToken, string itemId, int version, string hideVersion, string language)
        {

            Console.WriteLine($"Try to update __Hide version field for item {itemId} version {version}");

            // Call GraphQL endpoint here, specifying return data type, endpoint, method, query, and variables
            var result = await AuthoringGraphQl.ExecuteAsync<UpdateItemResponse>(
                context,
                $@"mutation UpdateItem {{
                    updateItem(
                        input: {{
                            version: {version}
                            itemId: ""{itemId}"",
                            language: ""{language}"",
                            fields: [
                                {inputFieldFormatAllowEmpty("__Hide version", hideVersion)}
                            ]
                        }}
                    ) {{
                        item {{
                            itemId
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
            Console.WriteLine($"Item updated Id: {result.Data.updateItem.item.itemId} ");
            return result.Data.updateItem.item;
         }


        internal static string inputFieldFormat(string name, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }
            return "\r\n {name: \"" + name + "\", value: \"" + HttpUtility.JavaScriptStringEncode(value) + "\" }";
        }

        internal static string inputFieldFormatAllowEmpty(string name, string value)
        {
            return "\r\n {name: \"" + name + "\", value: \"" + HttpUtility.JavaScriptStringEncode(value) + "\" }";
        }

    }
}

