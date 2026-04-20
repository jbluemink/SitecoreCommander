using System.Web;
using SitecoreCommander.Login;
using SitecoreCommander.Authoring.Model;
using System.Net;

namespace SitecoreCommander.Authoring
{ 
    //Example createpage, based on itemService item (usecase content migration)
    internal class UpdateItem
    {

        internal static async Task<Created?> Update(EnvironmentConfiguration env, CancellationToken cancellationToken, string itemId, string language, Dictionary<string, string> fields)
        {
            return await Update(AuthoringApiContext.FromEnvironment(env), cancellationToken, itemId, language, fields);
        }

        internal static async Task<Created?> Update(JwtTokenResponse token, string host, CancellationToken cancellationToken, string itemId, string language, Dictionary<string, string> fields)
        {
            return await Update(AuthoringApiContext.FromJwt(token, host), cancellationToken, itemId, language, fields);
        }

        internal static async Task<Created?> Update(JwtContext context, CancellationToken cancellationToken, string itemId, string language, Dictionary<string, string> fields)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            return await Update(AuthoringApiContext.FromJwt(
                new JwtTokenResponse { access_token = context.AccessToken }, 
                context.Host), cancellationToken, itemId, language, fields);
        }

        internal static async Task<Created?> Update(JwtContext context, CancellationToken cancellationToken, string itemId, string language, Dictionary<string, string> fields, HashSet<string>? allowEmptyFields)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            return await Update(AuthoringApiContext.FromJwt(
                new JwtTokenResponse { access_token = context.AccessToken }, 
                context.Host), cancellationToken, itemId, language, fields, allowEmptyFields);
        }

        private static async Task<Created?> Update(AuthoringApiContext context, CancellationToken cancellationToken, string itemId, string language, Dictionary<string, string> fields, HashSet<string>? allowEmptyFields = null)
        {

            Console.WriteLine("Try to update some field for item " + itemId);
            string graphQLfields = string.Empty;
            foreach (var field in fields)
            {
                var useAllowEmpty = allowEmptyFields != null && allowEmptyFields.Contains(field.Key);
                graphQLfields += useAllowEmpty
                    ? inputFieldFormatAllowEmpty(field.Key, field.Value)
                    : inputFieldFormat(field.Key, field.Value);
            }
            // Call GraphQL endpoint here, specifying return data type, endpoint, method, query, and variables
            var result = await AuthoringGraphQl.ExecuteAsync<UpdateItemResponse>(
                context,
                "mutation UpdateItem {" +
                "updateItem(" +
                "input: {" +
                "\r\n itemId: \"" + itemId + "\"" +
                "\r\n language: \"" + language + "\"" +
                "\r\n fields: [" +
                graphQLfields +
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

