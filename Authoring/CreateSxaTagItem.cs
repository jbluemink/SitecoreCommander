using System.Web;
using SitecoreCommander.Authoring.Model;
using SitecoreCommander.Login;
using SitecoreCommander.Authoring;

namespace SitecoreCommander.Authoring
{
    internal class CreateTagItem
    {
        public static string TagTemplateID = "{6B40E84C-8785-49FC-8A10-6BCA862FF7EA}";

        internal static async Task<Created?> CreateTag(EnvironmentConfiguration env, CancellationToken cancellationToken, string tag, string parentID, string language)
        {
            return await CreateTag(AuthoringApiContext.FromEnvironment(env), cancellationToken, tag, parentID, language);
        }

        internal static async Task<Created?> CreateTag(JwtTokenResponse token, string host, CancellationToken cancellationToken, string tag, string parentID, string language)
        {
            return await CreateTag(AuthoringApiContext.FromJwt(token, host), cancellationToken, tag, parentID, language);
        }

        internal static async Task<Created?> CreateTag(JwtContext context, CancellationToken cancellationToken, string tag, string parentID, string language)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            return await CreateTag(AuthoringApiContext.FromJwt(
                new JwtTokenResponse { access_token = context.AccessToken }, 
                context.Host), cancellationToken, tag, parentID, language);
        }

        private static async Task<Created?> CreateTag(AuthoringApiContext context, CancellationToken cancellationToken, string tag, string parentID, string language)
        {
            string itemname = Helper.ToValidItemName(tag);
            string templateId = TagTemplateID;
            Console.WriteLine("Try to Create tag item " + itemname);

            // Call GraphQL endpoint here, specifying return data type, endpoint, method, query, and variables
            var result = await AuthoringGraphQl.ExecuteAsync<CreateItem>(
                context,
                "mutation {" +
                "createItem(" +
                "input: {" +
                "name: \"" + itemname + "\"" +
                "\r\n templateId: \"" + templateId + "\"" +
                "\r\n parent: \"" + parentID + "\"" +
                "\r\n language: \"" + language + "\"" +
                "\r\n fields: [" +
                inputFieldFormat("Title", tag) +
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
            Console.WriteLine($"Tag Item created with Id: {result.Data.createItem.item.itemId} ");

            return result.Data.createItem.item;
        }


        internal static string inputFieldFormat(string name, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }
            return "\r\n {name: \"" + name + "\", value: \"" + HttpUtility.JavaScriptStringEncode(value) + "\" }";
        }

    }
}

