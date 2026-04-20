using System.Web;
using SitecoreCommander.Login;
using SitecoreCommander.Authoring.Model;
using SitecoreCommander.RESTful.Model;
using SitecoreCommander.RESTful;
using System.Net;


namespace SitecoreCommander.Authoring
{ 
    //Example createpage, based on itemService item (usecase content migration)
    internal class CreateMigratedSxaPage
    {

        /// <summary>
        /// Sample method which calls a GraphQL endpoint.
        /// </summary>
        internal static async Task<Created?> CreateLabelPageItem(EnvironmentConfiguration env, CookieContainer cookies, CancellationToken cancellationToken, string itemname, Guid parentID, StandardSscItemExtended xpItem, string templateID, string[] additionalLanguages)
        {
            return await CreateLabelPageItem(AuthoringApiContext.FromEnvironment(env), cookies, cancellationToken, itemname, parentID, xpItem, templateID, additionalLanguages);
        }

        internal static async Task<Created?> CreateLabelPageItem(JwtTokenResponse token, string host, CookieContainer cookies, CancellationToken cancellationToken, string itemname, Guid parentID, StandardSscItemExtended xpItem, string templateID, string[] additionalLanguages)
        {
            return await CreateLabelPageItem(AuthoringApiContext.FromJwt(token, host), cookies, cancellationToken, itemname, parentID, xpItem, templateID, additionalLanguages);
        }

        internal static async Task<Created?> CreateLabelPageItem(JwtContext context, CookieContainer cookies, CancellationToken cancellationToken, string itemname, Guid parentID, StandardSscItemExtended xpItem, string templateID, string[] additionalLanguages)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            return await CreateLabelPageItem(AuthoringApiContext.FromJwt(
                new JwtTokenResponse { access_token = context.AccessToken }, 
                context.Host), cookies, cancellationToken, itemname, parentID, xpItem, templateID, additionalLanguages);
        }

        private static async Task<Created?> CreateLabelPageItem(AuthoringApiContext context, CookieContainer cookies, CancellationToken cancellationToken, string itemname, Guid parentID, StandardSscItemExtended xpItem, string templateID, string[] additionalLanguages)
        {
            string language = Config.DefaultLanguage;

            Console.WriteLine("Try to Create item " + itemname);

            // Call GraphQL endpoint here, specifying return data type, endpoint, method, query, and variables
            var result = await AuthoringGraphQl.ExecuteAsync<CreateItem>(
                context,
                "mutation {" +
                "createItem(" +
                "input: {" +
                "name: \"" + itemname + "\"" +
                "\r\n templateId: \"" + templateID + "\"" +
                "\r\n parent: \"" + parentID.ToString() + "\"" +
                "\r\n language: \"" + language + "\"" +
                "\r\n fields: [" +
                inputFieldFormat("__Display name", xpItem.__DisplayName != xpItem.ItemName ? xpItem.__DisplayName : "") +
                inputFieldFormat("__Sortorder", xpItem.__Sortorder) +
                inputFieldFormat("__Created by", xpItem.__CreatedBy) +
                inputFieldFormat("__Created", xpItem.__Created) +
                inputFieldFormat("__Updated by", "SitecoreCommander") +
                inputFieldFormatAllowEmpty("__AutoThumbnails", "") +
                inputFieldFormat("__Hidden", xpItem.__Hidden) +
                inputFieldFormat("title", xpItem.Title) +
                inputFieldFormat("Content", xpItem.Content) +
                inputFieldFormat("NavigationTitle", xpItem.LinkCaptionInNavigation) +
                inputFieldFormat("SxaTags", xpItem.SxaTags) +
                inputFieldFormat("__Renderings", xpItem.MigratedRenderingenToXmCloud) +
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
            foreach(string additionallanguage in additionalLanguages)
            {
                var translate = await TranslatePageItem(context, cookies, cancellationToken, result.Data.createItem.item.itemId, xpItem, additionallanguage);
            }
            return result.Data.createItem.item;
        }

        internal static async Task<Created?> TranslatePageItem(EnvironmentConfiguration env, CookieContainer cookies, CancellationToken cancellationToken, string xmClouditemId, StandardSscItemExtended xpItem, string language)
        {
            return await TranslatePageItem(AuthoringApiContext.FromEnvironment(env), cookies, cancellationToken, xmClouditemId, xpItem, language);
        }

        internal static async Task<Created?> TranslatePageItem(JwtTokenResponse token, string host, CookieContainer cookies, CancellationToken cancellationToken, string xmClouditemId, StandardSscItemExtended xpItem, string language)
        {
            return await TranslatePageItem(AuthoringApiContext.FromJwt(token, host), cookies, cancellationToken, xmClouditemId, xpItem, language);
        }

        internal static async Task<Created?> TranslatePageItem(JwtContext context, CookieContainer cookies, CancellationToken cancellationToken, string xmClouditemId, StandardSscItemExtended xpItem, string language)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            return await TranslatePageItem(AuthoringApiContext.FromJwt(
                new JwtTokenResponse { access_token = context.AccessToken }, 
                context.Host), cookies, cancellationToken, xmClouditemId, xpItem, language);
        }

        private static async Task<Created?> TranslatePageItem(AuthoringApiContext context, CookieContainer cookies, CancellationToken cancellationToken, string xmClouditemId, StandardSscItemExtended xpItem, string language)
        {
            var xpSecondLanguage = await SscItemService.GetItemByIdAsync(xpItem.ItemID.ToString("B").ToUpper(), cookies, language);
            if (xpSecondLanguage == null || string.IsNullOrEmpty(xpSecondLanguage.__Revision))
            {
                return null;
            }
            var version = await AddItemVersion.Add(context, cancellationToken, xmClouditemId, language);
            return await UpdateVersionedFieldsLabelPageItem(context, cancellationToken, xmClouditemId, xpSecondLanguage, language);
        }

        internal static async Task<Created?> UpdateVersionedFieldsLabelPageItem(EnvironmentConfiguration env, CancellationToken cancellationToken, string itemId, StandardSscItemExtended xpItem, string language)
        {
            return await UpdateVersionedFieldsLabelPageItem(AuthoringApiContext.FromEnvironment(env), cancellationToken, itemId, xpItem, language);
        }

        internal static async Task<Created?> UpdateVersionedFieldsLabelPageItem(JwtTokenResponse token, string host, CancellationToken cancellationToken, string itemId, StandardSscItemExtended xpItem, string language)
        {
            return await UpdateVersionedFieldsLabelPageItem(AuthoringApiContext.FromJwt(token, host), cancellationToken, itemId, xpItem, language);
        }

        internal static async Task<Created?> UpdateVersionedFieldsLabelPageItem(JwtContext context, CancellationToken cancellationToken, string itemId, StandardSscItemExtended xpItem, string language)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            return await UpdateVersionedFieldsLabelPageItem(AuthoringApiContext.FromJwt(
                new JwtTokenResponse { access_token = context.AccessToken }, 
                context.Host), cancellationToken, itemId, xpItem, language);
        }

        private static async Task<Created?> UpdateVersionedFieldsLabelPageItem(AuthoringApiContext context, CancellationToken cancellationToken, string itemId, StandardSscItemExtended xpItem, string language)
        {
            Console.WriteLine("Try to Add version data for item " + itemId);

            // Call GraphQL endpoint here, specifying return data type, endpoint, method, query, and variables
            var result = await AuthoringGraphQl.ExecuteAsync<UpdateItemResponse>(
                context,
                "mutation UpdateItem {" +
                "updateItem(" +
                "input: {" +
                "\r\n itemId: \"" + itemId + "\"" +
                "\r\n language: \"" + language + "\"" +
                "\r\n fields: [" +
                inputFieldFormat("__Display name", xpItem.__DisplayName != xpItem.ItemName ? xpItem.__DisplayName : "") +
                inputFieldFormat("__Created by", xpItem.__CreatedBy) +
                inputFieldFormat("__Created", xpItem.__Created) +
                inputFieldFormat("__Updated by", "migrator") +
                inputFieldFormat("title", xpItem.Title) +
                inputFieldFormat("Content", xpItem.Content) +
                inputFieldFormat("NavigationTitle", xpItem.LinkCaptionInNavigation) +
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
            Console.WriteLine($"Item version created with Id: {result.Data.updateItem.item.itemId} ");
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

