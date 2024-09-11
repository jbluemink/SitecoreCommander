using System.Web;
using SitecoreCommander.Authoring.Model;
using SitecoreCommander.Edge.Model;
using SitecoreCommander.Edge;
using SitecoreCommander.Login;

namespace RaiXpToCloudMigrator.XmCloud
{
    internal class CreateFolderItem
    {
        public static string MediaFolderID = "{FE5DD826-48C6-436D-B87A-7C4210C7413B}";
        /// <summary>
        /// Sample method which calls a GraphQL endpoint.
        /// </summary>
        /// 
        internal static async Task<Created> CreateMapOrAddVersion(EnvironmentConfiguration env, CancellationToken cancellationToken, string itemname, string templateId, string parentID, string path, string language)
        {
            var createresult = await CreateMap(env, cancellationToken, itemname, templateId, parentID, language, new string[] { });
            if (createresult != null)
            {
                return createresult;
            }

            //creat is failed, check if item exist in en
            if (language != "en")
            {
                ResultGetItem itemen = await GetItem.GetSitecoreItem(env, cancellationToken, path, "en");

                if (itemen != null)
                {
                    var result = await Addversion(env, cancellationToken, itemname, templateId, itemen.id, language);
                    return result;
                }
            }
            return null;

        }

        internal static async Task<Created> CreateMap(EnvironmentConfiguration env, CancellationToken cancellationToken, string itemname, string templateId, string parentID, string language, string[] additionalLanguages)
        {
            string graphqlendpoint = env.Host;
            if (!graphqlendpoint.EndsWith("/")) { graphqlendpoint += "/"; }
            graphqlendpoint += "sitecore/api/authoring/graphql/v1/";
            string accessToken = env.AccessToken;

            Console.WriteLine("Try to Create item " + itemname);

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
            Console.WriteLine($"Folder Item created with Id: {result.Data.createItem.item.itemId} ");
            foreach (string additionallanguage in additionalLanguages)
            {
                var translate = await Addversion(env, cancellationToken, itemname, templateId, result.Data.createItem.item.itemId, language);
            }
            return result.Data.createItem.item;
        }

        internal static async Task<Created> CreateHiddenDataFolderEnglish(EnvironmentConfiguration env, CancellationToken cancellationToken, string itemname, string parentID)
        {
            string graphqlendpoint = env.Host;
            if (!graphqlendpoint.EndsWith("/")) { graphqlendpoint += "/"; }
            graphqlendpoint += "sitecore/api/authoring/graphql/v1/";
            string accessToken = env.AccessToken;

            Console.WriteLine("Try to Create item " + itemname);

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
                "\r\n templateId: \"{1C82E550-EBCD-4E5D-8ABD-D50D0809541E}\"" +
                "\r\n parent: \"" + parentID + "\"" +
                "\r\n language: \"en\"" +
                "\r\n fields: [" +
                inputFieldFormat("__Hidden", "1") +
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
            Console.WriteLine($"Data Folder created with Id: {result.Data.createItem.item.itemId} ");
            return result.Data.createItem.item;
        }


        internal static async Task<Created> Addversion(EnvironmentConfiguration env, CancellationToken cancellationToken, string itemname, string templateId, string itemID, string language)
        {
            string graphqlendpoint = env.Host;
            if (!graphqlendpoint.EndsWith("/")) { graphqlendpoint += "/"; }
            graphqlendpoint += "sitecore/api/authoring/graphql/v1/";
            string accessToken = env.AccessToken;

            Console.WriteLine("Try to add version " + itemname);

            // Call GraphQL endpoint here, specifying return data type, endpoint, method, query, and variables
            var result = await Request.CallGraphQLAsync<CreateItem>(
                new Uri(graphqlendpoint),
                HttpMethod.Post,
                accessToken,
                "",
                "mutation addItemVersion {" +
                "addItemVersion(" +
                "input: {" +
                "name: \"" + itemname + "\"" +
                "\r\n itemId: \"" + itemID + "\"" +
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
            Console.WriteLine($"Version created on item Id: {result.Data.createItem.item.itemId} ");
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

