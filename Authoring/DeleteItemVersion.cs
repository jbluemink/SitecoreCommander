using SitecoreCommander.Authoring.Model;
using SitecoreCommander.Edge;
using SitecoreCommander.Login;

namespace SitecoreCommander.Authoring
{
    internal class DeleteItemVersion
    {
        internal static async Task<ResultItem?> DeleteAll(EnvironmentConfiguration env, CancellationToken cancellationToken, string itemId, string language)
        {
            return await DeleteAll(AuthoringApiContext.FromEnvironment(env), cancellationToken, itemId, language);
        }

        internal static async Task<ResultItem?> DeleteAll(JwtTokenResponse token, string host, CancellationToken cancellationToken, string itemId, string language)
        {
            return await DeleteAll(AuthoringApiContext.FromJwt(token, host), cancellationToken, itemId, language);
        }

        internal static async Task<ResultItem?> DeleteAll(JwtContext context, CancellationToken cancellationToken, string itemId, string language)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            return await DeleteAll(AuthoringApiContext.FromJwt(
                new JwtTokenResponse { access_token = context.AccessToken }, 
                context.Host), cancellationToken, itemId, language);
        }

        private static async Task<ResultItem?> DeleteAll(AuthoringApiContext context, CancellationToken cancellationToken, string itemId, string language)
        {
            ResultItem? result;
            do
            {
                result = await Delete(context, cancellationToken, itemId, language, "");
            } while (result != null && !cancellationToken.IsCancellationRequested);

            return result;
        }

        internal static async Task<ResultItem?> Delete(EnvironmentConfiguration env, CancellationToken cancellationToken, string itemId, string language, string version)
        {
            return await Delete(AuthoringApiContext.FromEnvironment(env), cancellationToken, itemId, language, version);
        }

        internal static async Task<ResultItem?> Delete(JwtTokenResponse token, string host, CancellationToken cancellationToken, string itemId, string language, string version)
        {
            return await Delete(AuthoringApiContext.FromJwt(token, host), cancellationToken, itemId, language, version);
        }

        internal static async Task<ResultItem?> Delete(JwtContext context, CancellationToken cancellationToken, string itemId, string language, string version)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            return await Delete(AuthoringApiContext.FromJwt(
                new JwtTokenResponse { access_token = context.AccessToken }, 
                context.Host), cancellationToken, itemId, language, version);
        }

        private static async Task<ResultItem?> Delete(AuthoringApiContext context, CancellationToken cancellationToken, string itemId, string language, string version)
        {

            Console.WriteLine($@"Try to Delete version {version} {language} from item {itemId}");

            // Call GraphQL endpoint here, specifying return data type, endpoint, method, query, and variables
            var result = await AuthoringGraphQl.ExecuteAsync<SitecoreCommander.Authoring.Model.DeleteItemVersion>(
                context,
                $@"mutation deleteItemVersion{{
deleteItemVersion(
 input: {{
   itemId: ""{itemId}"" 
   language: ""{language}""
   {EdgeHelper.QueryFormatIntRemoveIfEmpty("version",version)}
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
            Console.WriteLine($"Item version deleted");
            return result.Data.deleteItemVersion.item;
        }


    }
}

