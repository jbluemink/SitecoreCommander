using System;
using System.Web;
using SitecoreCommander.Authoring.Model;
using SitecoreCommander.Login;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SitecoreCommander.Authoring
{
    internal class AddMedia
    {
        public static string JpegTemplateID = "{DAF085E8-602E-43A6-8299-038FF171349F}";
        public static string ImageTemplateID = "{DAF085E8-602E-43A6-8299-038FF171349F}";
        public static string FileTemplateID = "{962B53C4-F93B-4DF9-9821-415C867B8903}";
        //Note
        //If the query for a pre-signed upload URL returns an error with the message The specified key is not a valid size for this algorithm, check if the GraphQL.UploadMediaOptions.EncryptionKey setting has a value. For example:
        //<setting name = "GraphQL.UploadMediaOptions.EncryptionKey" value= "432A462D4A614E64" />
        //If it does not, you must set it.
        internal static async Task<Uploaded> Create(EnvironmentConfiguration env, CancellationToken cancellationToken, string websiteitempath, string language, string alt, string file)
        {
            string graphqlendpoint = env.Host;
            if (!graphqlendpoint.EndsWith("/")) { graphqlendpoint += "/"; }
            graphqlendpoint += "sitecore/api/authoring/graphql/v1/";
            string accessToken = env.AccessToken;

            Console.WriteLine("Try to Create media item " + websiteitempath);

            // Call GraphQL endpoint here, specifying return data type, endpoint, method, query, and variables
            var result = await Request.CallGraphQLAsync<UploadMedia>(
                new Uri(graphqlendpoint),
                HttpMethod.Post,
                accessToken,
                "",
                "mutation {" +
                "uploadMedia(" +
                "input: {" +
                "itemPath: \"" + websiteitempath.Replace("/sitecore/media library/","") + "\"" +
                "\r\n alt: \"" + alt + "\"" +
                "\r\n language: \"" + language + "\"" +
               "\r\n}\r\n  ) {\r\n    presignedUploadUrl }\r\n}",
                new
                {
                },
                cancellationToken);

            // Examine the GraphQL response to see if any errors were encountered
            if (result.Errors?.Count > 0)
            {
                Console.WriteLine($"GraphQL returned errors:\n{string.Join("\n", result.Errors.Select(x => $"  - {x.Message}"))}");
                if (result.Errors.FirstOrDefault().Message == "The specified key is not a valid size for this algorithm.")
                {
                    Console.WriteLine("TIP: Check if the GraphQL.UploadMediaOptions.EncryptionKey setting has a value. For example: <setting name=\"GraphQL.UploadMediaOptions.EncryptionKey\" value=\"432A462D4A614E64\" />");
                }
                return null;
            }

            // Use the response data
            Console.WriteLine($"Media created upload to: {result.Data.uploadMedia.presignedUploadUrl} ");

            var result2 = await Request.CallGraphQLUploadAsync<Uploaded>(
                           HttpMethod.Post,
                           accessToken,
                           result.Data.uploadMedia.presignedUploadUrl,
                           file,
                           cancellationToken);

            // Examine the response to see if any errors were encountered
            if (!string.IsNullOrEmpty(result2.Message))
            {
                Console.WriteLine($"upload returned errors:{result2.Message}");
                return null;
            }

            Console.WriteLine($"media uploaded id=: {result2.Id}");
            return result2;
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

