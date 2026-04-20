using SitecoreCommander.ContentHub.Model;
using Stylelabs.M.Base.Querying;
using Stylelabs.M.Base.Querying.Linq;
using Stylelabs.M.Sdk.Contracts.Base;
using Stylelabs.M.Sdk.WebClient;

namespace SitecoreCommander.ContentHub.Services
{
    internal sealed class ContentHubTaxonomyService
    {
        private readonly IWebMClient _client;
        private readonly ContentHubOptions _options;

        internal ContentHubTaxonomyService(IWebMClient client, ContentHubOptions options)
        {
            _client = client;
            _options = options;
        }

        internal async Task<IEntity?> FindByNameAsync(string definitionName, string name)
        {
            var query = Query.CreateQuery(entities =>
                from e in entities
                where e.DefinitionName == definitionName
                where e.Property(_options.TaxonomyNamePropertyName) == name
                select e);

            var result = await _client.Querying.QueryAsync(query);
            return result.Items.FirstOrDefault();
        }
    }
}
