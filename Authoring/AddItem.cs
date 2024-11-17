using System.Web;
using SitecoreCommander.Authoring.Model;
using SitecoreCommander.Login;

namespace RaiXpToCloudMigrator.XmCloud
{
    internal class AddItem
    {
        public static string SampleItemTemplateID = "{76036F5E-CBCE-46D1-AF0A-4143F9B557AA}";

        internal static async Task<Created> Create(EnvironmentConfiguration env, CancellationToken cancellationToken, string itemname, string templateId, string parentID, string language, Dictionary<string, string> fields)
        {
            string graphqlendpoint = env.Host;
            if (!graphqlendpoint.EndsWith("/")) { graphqlendpoint += "/"; }
            graphqlendpoint += "sitecore/api/authoring/graphql/v1/";
            string accessToken = env.AccessToken;

            Console.WriteLine("Try to Create item " + itemname);
            string graphQLfields = string.Empty;
            foreach (var field in fields)
            {
                graphQLfields += inputFieldFormat(field.Key, field.Value);
            }
            // Call GraphQL endpoint here, specifying return data type, endpoint, method, query, and variables
            var result = await Request.CallGraphQLAsync<CreateItem>(
                new Uri(graphqlendpoint),
                HttpMethod.Post,
                accessToken,
                "",
                "mutation {" +
                "createItem(" +
                "input: {" +
                "name: \"" + itemname + "\"" +
                "\r\n templateId: \"" + templateId + "\"" +
                "\r\n parent: \"" + parentID + "\"" +
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
            Console.WriteLine($"Item created with Id: {result.Data.createItem.item.itemId} ");
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

