using SitecoreCommander.Authoring.Model;
using SitecoreCommander.Login;

namespace SitecoreCommander.Authoring
{
    /// <summary>
    /// Retrieves a single content item with all its fields from the Authoring API.
    /// Supports field pagination for items with more than 50 fields.
    /// </summary>
    public class GetItemWithAllFields
    {
        /// <summary>
    /// Get a single item by path with configurable field filtering.
    /// </summary>
    /// <param name="env">Environment configuration</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <param name="itemPath">Item path</param>
    /// <param name="language">Language</param>
    /// <param name="ownFields">Only include fields defined on the item itself (default: false)</param>
    /// <param name="excludeStandardFields">Exclude Sitecore standard fields like __Created, __Updated (default: false)</param>
    public static async Task<ResultItemWithAllFields?> Get(EnvironmentConfiguration env, CancellationToken cancellationToken, string itemPath, string language, bool ownFields = false, bool excludeStandardFields = false)
    {
        return await Get(AuthoringApiContext.FromEnvironment(env), cancellationToken, itemPath, language, ownFields, excludeStandardFields);
    }

    /// <summary>
    /// Get a single item by path with configurable field filtering (JWT auth).
    /// </summary>
    public static async Task<ResultItemWithAllFields?> Get(JwtTokenResponse token, string host, CancellationToken cancellationToken, string itemPath, string language, bool ownFields = false, bool excludeStandardFields = false)
    {
        return await Get(AuthoringApiContext.FromJwt(token, host), cancellationToken, itemPath, language, ownFields, excludeStandardFields);
    }

    /// <summary>
    /// Get a single item by path with configurable field filtering (JWT auth via JwtContext).
    /// </summary>
    public static async Task<ResultItemWithAllFields?> Get(JwtContext context, CancellationToken cancellationToken, string itemPath, string language, bool ownFields = false, bool excludeStandardFields = false)
    {
        if (context == null)
            throw new ArgumentNullException(nameof(context));
        return await Get(AuthoringApiContext.FromJwt(
            new JwtTokenResponse { access_token = context.AccessToken }, 
            context.Host), cancellationToken, itemPath, language, ownFields, excludeStandardFields);
    }

    /// <summary>
    /// Get item by ID with configurable field filtering.
    /// </summary>
    /// <param name="env">Environment configuration</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <param name="itemId">Item ID</param>
    /// <param name="language">Language</param>
    /// <param name="ownFields">Only include fields defined on the item itself (default: false)</param>
    /// <param name="excludeStandardFields">Exclude Sitecore standard fields like __Created, __Updated (default: false)</param>
    internal static async Task<ResultItemWithAllFields?> GetById(EnvironmentConfiguration env, CancellationToken cancellationToken, string itemId, string language, bool ownFields = false, bool excludeStandardFields = false)
    {
        return await GetById(AuthoringApiContext.FromEnvironment(env), cancellationToken, itemId, language, ownFields, excludeStandardFields);
    }

    /// <summary>
    /// Get item by ID with configurable field filtering (JWT auth).
    /// </summary>
    internal static async Task<ResultItemWithAllFields?> GetById(JwtTokenResponse token, string host, CancellationToken cancellationToken, string itemId, string language, bool ownFields = false, bool excludeStandardFields = false)
    {
        return await GetById(AuthoringApiContext.FromJwt(token, host), cancellationToken, itemId, language, ownFields, excludeStandardFields);
    }

    /// <summary>
    /// Get item by ID with configurable field filtering (JWT auth via JwtContext).
    /// </summary>
    internal static async Task<ResultItemWithAllFields?> GetById(JwtContext context, CancellationToken cancellationToken, string itemId, string language, bool ownFields = false, bool excludeStandardFields = false)
    {
        if (context == null)
            throw new ArgumentNullException(nameof(context));
        return await GetById(AuthoringApiContext.FromJwt(
            new JwtTokenResponse { access_token = context.AccessToken }, 
            context.Host), cancellationToken, itemId, language, ownFields, excludeStandardFields);
    }

    private static async Task<ResultItemWithAllFields?> Get(AuthoringApiContext context, CancellationToken cancellationToken, string itemPath, string language, bool ownFields = false, bool excludeStandardFields = false)
        {
            Console.WriteLine($"Retrieving item with all fields from path: {itemPath} (language: {language}, ownFields: {ownFields}, excludeStandard: {excludeStandardFields})");

            // Step 1: Get basic item info and first batch of fields
            var itemWithFields = await GetItemWithFieldsPaginated(context, cancellationToken, itemPath, language, "path", ownFields, excludeStandardFields);

            if (itemWithFields == null)
            {
                Console.WriteLine($"Item not found at path: {itemPath}");
                return null;
            }

            Console.WriteLine($"Item fetched: {itemWithFields.name} (ID: {itemWithFields.itemId}, version: {itemWithFields.version})");
            Console.WriteLine($"Total fields in item: {itemWithFields.fields.Count}");

            return itemWithFields;
        }

        private static async Task<ResultItemWithAllFields?> GetById(AuthoringApiContext context, CancellationToken cancellationToken, string itemId, string language, bool ownFields = false, bool excludeStandardFields = false)
        {
            Console.WriteLine($"Retrieving item with all fields by ID: {itemId} (language: {language}, ownFields: {ownFields}, excludeStandard: {excludeStandardFields})");

            // Step 1: Get basic item info and first batch of fields
            var itemWithFields = await GetItemWithFieldsPaginated(context, cancellationToken, itemId, language, "id", ownFields, excludeStandardFields);

            if (itemWithFields == null)
            {
                Console.WriteLine($"Item not found with ID: {itemId}");
                return null;
            }

            Console.WriteLine($"Item fetched: {itemWithFields.name} (ID: {itemWithFields.itemId}, version: {itemWithFields.version})");
            Console.WriteLine($"Total fields in item: {itemWithFields.fields.Count}");

            return itemWithFields;
        }

        /// <summary>
        /// Internal method that handles field pagination.
        /// The Authoring GraphQL API has a default limit of 50 fields per page.
        /// This method iterates through all pages to collect all fields.
        /// </summary>
        private static async Task<ResultItemWithAllFields?> GetItemWithFieldsPaginated(
            AuthoringApiContext context,
            CancellationToken cancellationToken,
            string itemPathOrId,
            string language,
            string queryType,
            bool ownFields = false,
            bool excludeStandardFields = false)
        {
            var result = new ResultItemWithAllFields();
            var allFields = new Dictionary<string, ResultValue>();
            string? endCursor = null;
            int pageIndex = 0;

            do
            {
                pageIndex++;
                Console.WriteLine($"Fetching field page {pageIndex}...");

                // Build the query - use different where clause based on queryType
                string whereClause = queryType == "id"
                    ? $@"itemId: ""{itemPathOrId}"""
                    : $@"path: ""{itemPathOrId}""";

                string afterCursor = !string.IsNullOrEmpty(endCursor)
                    ? $@", after: ""{endCursor}"""
                    : string.Empty;

                // Build field filters
                string fieldFilters = $"first: 50, ownFields: {ownFields.ToString().ToLower()}, excludeStandardFields: {excludeStandardFields.ToString().ToLower()}{afterCursor}";

                // The Authoring GraphQL API field query with pagination support
                string query = $@"query {{
  item(where: {{{whereClause}}}) {{
    itemId
    name
    path
    version
    language {{
      name
    }}
    created: field(name: ""__Created"") {{
      value
    }}
    updated: field(name: ""__Updated"") {{
      value
    }}
    security: field(name: ""__Security"") {{
      value
    }}
    template {{
      templateId
      name
    }}
    access {{
      canWrite
      canDelete
      canRename
    }}
    fields({fieldFilters}) {{
      pageInfo {{
        hasNextPage
        endCursor
      }}
      nodes {{
                fieldId
        name
        value
      }}
    }}
  }}
}}";

                var pageResult = await AuthoringGraphQl.ExecuteAsync<ResultItemAllFieldsWithConnectionResponse>(
                    context,
                    query,
                    new { },
                    cancellationToken);

                if (pageResult.Errors?.Count > 0)
                {
                    Console.WriteLine($"GraphQL returned errors:\n{string.Join("\n", pageResult.Errors.Select(x => $"  - {x.Message}"))}");
                    return null;
                }

                if (pageResult.Data?.item == null)
                {
                    Console.WriteLine("Item not found in response.");
                    return null;
                }

                var itemData = pageResult.Data.item;

                // On first page, copy the basic item info
                if (pageIndex == 1)
                {
                    result.itemId = itemData.itemId;
                    result.name = itemData.name;
                    result.path = itemData.path;
                    result.language = itemData.language;
                    result.version = itemData.version;
                    result.created = itemData.created;
                    result.updated = itemData.updated;
                    result.security = itemData.security;
                    result.template = itemData.template;
                    result.access = itemData.access;
                }

                // Collect all fields from this page
                if (itemData.fields?.nodes != null)
                {
                    foreach (var fieldNode in itemData.fields.nodes)
                    {
                        if (!string.IsNullOrEmpty(fieldNode.name))
                        {
                            allFields[fieldNode.name] = new ResultValue
                            {
                                value = fieldNode.value ?? string.Empty
                            };
                        }
                    }

                    Console.WriteLine($"  Page {pageIndex}: {itemData.fields.nodes.Length} fields retrieved");

                    // Check if there are more pages
                    if (itemData.fields.pageInfo.hasNextPage)
                    {
                        endCursor = itemData.fields.pageInfo.endCursor;
                    }
                    else
                    {
                        endCursor = null;
                    }
                }
                else
                {
                    endCursor = null;
                }

            } while (!string.IsNullOrEmpty(endCursor));

            result.fields = allFields;
            return result;
        }
    }
}
