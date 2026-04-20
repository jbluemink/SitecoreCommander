using System.Web;
using SitecoreCommander.Login;
using SitecoreCommander.Authoring.Model;
using System.Net;

namespace SitecoreCommander.Authoring
{ 
    //Example createpage, based on itemService item (usecase content migration)
    internal class UpdateItemSecurity
    {

        internal static async Task<Created?> UpdateItem(EnvironmentConfiguration env, CancellationToken cancellationToken, string itemId, string securityString, string language)
        {
            return await UpdateItem(AuthoringApiContext.FromEnvironment(env), cancellationToken, itemId, securityString, language);
        }

        internal static async Task<Created?> UpdateItem(JwtTokenResponse token, string host, CancellationToken cancellationToken, string itemId, string securityString, string language)
        {
            return await UpdateItem(AuthoringApiContext.FromJwt(token, host), cancellationToken, itemId, securityString, language);
        }

        internal static async Task<Created?> UpdateItem(JwtContext context, CancellationToken cancellationToken, string itemId, string securityString, string language)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            return await UpdateItem(AuthoringApiContext.FromJwt(
                new JwtTokenResponse { access_token = context.AccessToken }, 
                context.Host), cancellationToken, itemId, securityString, language);
        }

        private static async Task<Created?> UpdateItem(AuthoringApiContext context, CancellationToken cancellationToken, string itemId, string securityString, string language)
        {

            Console.WriteLine("Try to update security field for item " + itemId);

            // Call GraphQL endpoint here, specifying return data type, endpoint, method, query, and variables
            var result = await AuthoringGraphQl.ExecuteAsync<UpdateItemResponse>(
                context,
                "mutation UpdateItem {" +
                "updateItem(" +
                "input: {" +
                "\r\n itemId: \"" + itemId + "\"" +
                "\r\n language: \"" + language + "\"" +
                "\r\n fields: [" +
                inputFieldFormatAllowEmpty("__Security", securityString) +
                "\r\n      ]\r\n    }\r\n  ) {\r\n    item {\r\n      itemId\r\n    }\r\n  }\r\n}",
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

