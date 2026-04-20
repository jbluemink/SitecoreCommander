using SitecoreCommander.ContentHub.Model;
using SitecoreCommander.Utils;
using Stylelabs.M.Base.Querying;
using Stylelabs.M.Base.Querying.Linq;
using Stylelabs.M.Framework.Essentials.LoadConfigurations;
using Stylelabs.M.Framework.Essentials.LoadOptions;
using Stylelabs.M.Sdk.Contracts.Base;
using Stylelabs.M.Sdk.WebClient;
using System.Reflection;

namespace SitecoreCommander.ContentHub.Services
{
    internal sealed class ContentHubAssetService : ContentHubAssetImportBaseService
    {
        internal ContentHubAssetService(IWebMClient client, ContentHubOptions options)
            : base(client, options)
        {
        }

        /// <summary>
        /// Returns up to <paramref name="maxCount"/> assets from Content Hub.
        /// </summary>
        internal async Task<List<IEntity>> GetAssetsAsync(int maxCount = 50)
        {
            var query = Query.CreateQuery(entities =>
                from e in entities
                where e.DefinitionName == Options.AssetDefinitionName
                select e);

            var result = await Client.Querying.QueryAsync(query);
            return result.Items.Take(maxCount).ToList();
        }

        /// <summary>
        /// Creates a new asset in Content Hub and optionally uploads the binary.
        /// </summary>
        internal async Task<ContentHubImportResult> CreateAssetAsync(
            ContentHubMediaMigrationItem media,
            ContentHubTaxonomySelection? taxonomy,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var entity = await Client.EntityFactory.CreateAsync(Options.AssetDefinitionName);

            TrySetPropertyValue(entity, Options.AssetFileNamePropertyName, media.FileName);
            TrySetPropertyValue(entity, Options.AssetTitlePropertyName, string.IsNullOrWhiteSpace(media.Title) ? media.FileName : media.Title);
            TrySetPropertyValue(entity, Options.AssetAltTextPropertyName, media.AltText);
            TrySetPropertyValue(entity, Options.AssetDescriptionPropertyName, media.Description);

            await SetStandardContentRepositoryAsync(entity);
            await SetApprovedLifecycleStatusAsync(entity);

            if (!string.IsNullOrWhiteSpace(taxonomy?.Category))
            {
                await SetChildToOneParentRelationByNameAsync(
                    entity,
                    Options.CategoryRelationName,
                    Options.CategoryDefinitionName,
                    Options.TaxonomyNamePropertyName,
                    taxonomy.Category);
            }

            await Client.Entities.SaveAsync(entity);

            var resolvedAssetId = entity.Id;

            media.ContentHubIdentifier = GetStringPropertyValue(entity, "Identifier");
            var publicHash = GetStringPropertyValue(entity, "PublicHash");
            if (string.IsNullOrWhiteSpace(publicHash))
                publicHash = GetStringPropertyValue(entity, "FileHash");
            media.ContentHubPublicHash = publicHash;
            media.ContentHubVersionHash = GetStringPropertyValue(entity, "VersionHash");

            if (!string.IsNullOrWhiteSpace(media.SourceUrl))
            {
                try
                {
                    var uploadedDirect = await TryUploadBinaryDirectAsync(
                        resolvedAssetId,
                        media.SourceUrl,
                        media.FileName,
                        cancellationToken);

                    if (!uploadedDirect)
                    {
                        await CreateFetchJobAsync(resolvedAssetId, media.SourceUrl);
                    }
                }
                catch (Exception ex)
                {
                    await SimpleLogger.LogAsync($"[ContentHub] fetch-job-error | assetId={resolvedAssetId} | error={ex.Message}");
                }
            }

            var publicReady = await EnsurePublicLinkHashesAsync(media, resolvedAssetId, cancellationToken);

            if (!publicReady && Options.RequirePublicLinks)
            {
                return new ContentHubImportResult
                {
                    Success = false,
                    AssetId = resolvedAssetId,
                    Created = true,
                    Updated = false,
                    Message = $"Asset created but public link hashes are missing for assetId={resolvedAssetId}."
                };
            }

            await SimpleLogger.LogAsync($"[ContentHub] asset-created | assetId={resolvedAssetId}");

            if (!resolvedAssetId.HasValue)
            {
                return new ContentHubImportResult
                {
                    Success = false,
                    AssetId = null,
                    Created = true,
                    Updated = false,
                    Message = "Asset saved but ID could not be resolved."
                };
            }

            return new ContentHubImportResult
            {
                Success = true,
                AssetId = resolvedAssetId,
                Created = true,
                Updated = false,
                Message = "Created"
            };
        }

        private async Task<bool> EnsurePublicLinkHashesAsync(
            ContentHubMediaMigrationItem media,
            long? assetId,
            CancellationToken cancellationToken)
        {
            if (!assetId.HasValue)
                return false;

            await RefreshPublicHashesFromPersistedAssetAsync(media, assetId.Value);
            if (!string.IsNullOrWhiteSpace(media.ContentHubPublicHash) && !string.IsNullOrWhiteSpace(media.ContentHubVersionHash))
                return true;

            if (!Options.EnsurePublicLinks)
            {
                await SimpleLogger.LogAsync($"[ContentHub] public-link-skip | assetId={assetId} | reason=disabled");
                return false;
            }

            var retries = Math.Max(1, Options.PublicLinkRetryCount);
            var delayMs = Math.Max(250, Options.PublicLinkRetryDelayMs);

            for (var attempt = 1; attempt <= retries; attempt++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var actionExecuted = await TryExecutePublicLinkActionAsync(assetId.Value);

                var created = false;
                if (!actionExecuted && !await PublicLinkExistsForAssetAsync(assetId.Value))
                {
                    created = await TryCreatePublicLinkEntityAsync(assetId.Value);
                }

                var updatedExisting = await EnsureExistingPublicLinksConfiguredAsync(assetId.Value);

                await RefreshPublicHashesFromPersistedAssetAsync(media, assetId.Value);

                if (!string.IsNullOrWhiteSpace(media.ContentHubPublicHash) && !string.IsNullOrWhiteSpace(media.ContentHubVersionHash))
                {
                    await SimpleLogger.LogAsync($"[ContentHub] public-link-ready | assetId={assetId.Value} | attempt={attempt} | actionExecuted={actionExecuted} | created={created} | updatedExisting={updatedExisting}");
                    return true;
                }

                if (updatedExisting)
                {
                    await SimpleLogger.LogAsync($"[ContentHub] public-link-ready-fallback | assetId={assetId.Value} | attempt={attempt} | reason=existing-link-updated");
                    return true;
                }

                if (attempt < retries)
                    await Task.Delay(delayMs, cancellationToken);
            }

            await SimpleLogger.LogAsync($"[ContentHub] public-link-missing | assetId={assetId.Value} | retries={retries}");
            return false;
        }

        private async Task<bool> PublicLinkExistsForAssetAsync(long assetId)
        {
            try
            {
                var query = Query.CreateQuery(entities =>
                    from e in entities
                    where e.DefinitionName == "M.PublicLink"
                    select e.LoadConfiguration(QueryLoadConfiguration.Default)
                            .WithRelations(new RelationLoadOption { LoadOption = LoadOption.All }));

                var result = await Client.Querying.QueryAsync(query);

                foreach (var publicLink in result.Items)
                {
                    try
                    {
                        var rel = publicLink.GetRelation<IChildToManyParentsRelation>("AssetToPublicLink");
                        if (rel?.Parents.Contains(assetId) == true)
                            return true;
                    }
                    catch (InvalidCastException) { }

                    try
                    {
                        var rel = publicLink.GetRelation<IChildToOneParentRelation>("AssetToPublicLink");
                        if (rel?.Parent == assetId)
                            return true;
                    }
                    catch (InvalidCastException) { }
                }

                return false;
            }
            catch (Exception ex)
            {
                await SimpleLogger.LogAsync($"[ContentHub] public-link-check-failed | assetId={assetId} | error={ex.Message}");
                return false;
            }
        }

        private async Task<bool> TryCreatePublicLinkEntityAsync(long assetId)
        {
            try
            {
                var publicLink = await Client.EntityFactory.CreateAsync("M.PublicLink");
                var resource = await ResolvePublicLinkResourceAsync(assetId);
                var relativeUrl = $"asset-{assetId}";

                var linked = false;

                try
                {
                    var rel = publicLink.GetRelation<IChildToManyParentsRelation>("AssetToPublicLink");
                    if (rel != null) { rel.Parents.Add(assetId); linked = true; }
                }
                catch (InvalidCastException) { }

                if (!linked)
                {
                    try
                    {
                        var rel = publicLink.GetRelation<IChildToOneParentRelation>("AssetToPublicLink");
                        if (rel != null) { rel.Parent = assetId; linked = true; }
                    }
                    catch (InvalidCastException) { }
                }

                if (!linked)
                {
                    await SimpleLogger.LogAsync($"[ContentHub] public-link-entity-skip | assetId={assetId} | reason=asset-relation-not-found");
                    return false;
                }

                try { publicLink.SetPropertyValue("Resource", resource); }
                catch (Exception ex) { await SimpleLogger.LogAsync($"[ContentHub] public-link-resource-set-failed | assetId={assetId} | error={ex.Message}"); }

                try { publicLink.SetPropertyValue("RelativeUrl", relativeUrl); }
                catch (Exception ex) { await SimpleLogger.LogAsync($"[ContentHub] public-link-relativeurl-set-failed | assetId={assetId} | value={relativeUrl} | error={ex.Message}"); }

                await Client.Entities.SaveAsync(publicLink).ConfigureAwait(false);

                try
                {
                    publicLink.SetPropertyValue("ConversionConfiguration", null);
                    await SimpleLogger.LogAsync($"[ContentHub] public-link-conversion-config-set | assetId={assetId} | value=null");
                }
                catch (Exception ex) { await SimpleLogger.LogAsync($"[ContentHub] public-link-conversion-config-set-failed | assetId={assetId} | error={ex.Message}"); }

                await Client.Entities.SaveAsync(publicLink).ConfigureAwait(false);
                await SimpleLogger.LogAsync($"[ContentHub] public-link-entity-created | assetId={assetId} | publicLinkId={publicLink.Id} | resource={resource} | relativeUrl={relativeUrl}");
                return true;
            }
            catch (Exception ex)
            {
                await SimpleLogger.LogAsync($"[ContentHub] public-link-entity-create-warning | assetId={assetId} | detail={ex.Message}");
                return false;
            }
        }

        private async Task<bool> EnsureExistingPublicLinksConfiguredAsync(long assetId)
        {
            try
            {
                var links = await GetPublicLinksForAssetAsync(assetId);
                if (links.Count == 0)
                    return false;

                var resource = await ResolvePublicLinkResourceAsync(assetId);
                var relativeUrl = $"asset-{assetId}";
                var updated = false;

                foreach (var link in links)
                {
                    var touched = false;

                    try { link.SetPropertyValue("Resource", resource); touched = true; }
                    catch (Exception ex) { await SimpleLogger.LogAsync($"[ContentHub] public-link-existing-resource-set-failed | assetId={assetId} | publicLinkId={link.Id} | error={ex.Message}"); }

                    try
                    {
                        var currentRelativeUrl = GetStringPropertyValue(link, "RelativeUrl");
                        if (string.IsNullOrWhiteSpace(currentRelativeUrl))
                        {
                            link.SetPropertyValue("RelativeUrl", relativeUrl);
                            touched = true;
                        }
                    }
                    catch (Exception ex) { await SimpleLogger.LogAsync($"[ContentHub] public-link-existing-relativeurl-set-failed | assetId={assetId} | publicLinkId={link.Id} | error={ex.Message}"); }

                    try { link.SetPropertyValue("ConversionConfiguration", null); touched = true; }
                    catch (Exception ex) { await SimpleLogger.LogAsync($"[ContentHub] public-link-existing-conversion-set-failed | assetId={assetId} | publicLinkId={link.Id} | error={ex.Message}"); }

                    if (touched)
                    {
                        await Client.Entities.SaveAsync(link).ConfigureAwait(false);
                        updated = true;
                    }
                }

                if (updated)
                    await SimpleLogger.LogAsync($"[ContentHub] public-link-existing-updated | assetId={assetId} | count={links.Count} | resource={resource}");

                return updated;
            }
            catch (Exception ex)
            {
                await SimpleLogger.LogAsync($"[ContentHub] public-link-existing-update-failed | assetId={assetId} | error={ex.Message}");
                return false;
            }
        }

        private async Task<List<IEntity>> GetPublicLinksForAssetAsync(long assetId)
        {
            var links = new List<IEntity>();

            var query = Query.CreateQuery(entities =>
                from e in entities
                where e.DefinitionName == "M.PublicLink"
                select e.LoadConfiguration(QueryLoadConfiguration.Default)
                        .WithRelations(new RelationLoadOption { LoadOption = LoadOption.All }));

            var result = await Client.Querying.QueryAsync(query);

            foreach (var publicLink in result.Items)
            {
                try
                {
                    var rel = publicLink.GetRelation<IChildToManyParentsRelation>("AssetToPublicLink");
                    if (rel?.Parents.Contains(assetId) == true)
                    {
                        links.Add(publicLink);
                        continue;
                    }
                }
                catch (InvalidCastException) { }

                try
                {
                    var rel = publicLink.GetRelation<IChildToOneParentRelation>("AssetToPublicLink");
                    if (rel?.Parent == assetId)
                        links.Add(publicLink);
                }
                catch (InvalidCastException) { }
            }

            return links;
        }

        private async Task<string> ResolvePublicLinkResourceAsync(long assetId)
        {
            try
            {
                await Client.Entities.GetAsync(assetId).ConfigureAwait(false);
                return "downloadOriginal";
            }
            catch
            {
                return "downloadOriginal";
            }
        }

        private async Task RefreshPublicHashesFromPersistedAssetAsync(ContentHubMediaMigrationItem media, long assetId)
        {
            var persisted = await Client.Entities.GetAsync(assetId);
            if (persisted == null)
                return;

            media.ContentHubIdentifier = GetStringPropertyValue(persisted, "Identifier");
            var publicHash = GetStringPropertyValue(persisted, "PublicHash");
            if (string.IsNullOrWhiteSpace(publicHash))
                publicHash = GetStringPropertyValue(persisted, "FileHash");
            media.ContentHubPublicHash = publicHash;
            media.ContentHubVersionHash = GetStringPropertyValue(persisted, "VersionHash");
        }

        private async Task<bool> TryExecutePublicLinkActionAsync(long assetId)
        {
            try
            {
                var actionHosts = new List<object> { Client };
                var clientProperties = Client.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
                foreach (var property in clientProperties)
                {
                    object? value = null;
                    try { value = property.GetValue(Client); }
                    catch { }

                    if (value == null)
                        continue;

                    var hostMethods = value.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public);
                    if (hostMethods.Any(m => string.Equals(m.Name, "ExecuteAsync", StringComparison.OrdinalIgnoreCase)
                                          || string.Equals(m.Name, "ExecuteActionAsync", StringComparison.OrdinalIgnoreCase)))
                    {
                        actionHosts.Add(value);
                    }
                }

                var methods = actionHosts
                    .SelectMany(h => h.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public)
                        .Where(m => string.Equals(m.Name, "ExecuteAsync", StringComparison.OrdinalIgnoreCase)
                                 || string.Equals(m.Name, "ExecuteActionAsync", StringComparison.OrdinalIgnoreCase))
                        .Select(m => new { Host = h, Method = m }))
                    .ToList();

                if (methods.Count == 0)
                {
                    await SimpleLogger.LogAsync($"[ContentHub] public-link-action-skip | assetId={assetId} | reason=actions-client-unavailable");
                    return false;
                }

                foreach (var actionName in GetPublicLinkActionCandidates())
                {
                    foreach (var parameterName in GetActionParameterNameCandidates())
                    {
                        var parameters = new Dictionary<string, object> { [parameterName] = assetId };

                        foreach (var entry in methods)
                        {
                            var args = BuildActionInvokeArgs(entry.Method.GetParameters(), actionName, parameters);
                            if (args == null)
                                continue;

                            var result = entry.Method.Invoke(entry.Host, args);
                            if (result is Task task)
                            {
                                await task.ConfigureAwait(false);
                                await SimpleLogger.LogAsync($"[ContentHub] public-link-action-executed | action={actionName} | assetId={assetId} | method={entry.Method.Name} | parameter={parameterName}");
                                return true;
                            }
                        }
                    }
                }

                await SimpleLogger.LogAsync($"[ContentHub] public-link-action-skip | assetId={assetId} | reason=no-working-action");
                return false;
            }
            catch (Exception ex)
            {
                await SimpleLogger.LogAsync($"[ContentHub] public-link-action-failed | assetId={assetId} | error={ex.Message}");
                return false;
            }
        }

        private IEnumerable<string> GetPublicLinkActionCandidates()
        {
            if (!string.IsNullOrWhiteSpace(Options.PublicLinkActionName))
                yield return Options.PublicLinkActionName.Trim();

            yield return "M.Asset.MakePublic";
            yield return "M.Asset.CreatePublicLink";
            yield return "M.Asset.GeneratePublicLink";
            yield return "M.Asset.GeneratePublicUrl";
            yield return "M.Asset.Publish";
        }

        private IEnumerable<string> GetActionParameterNameCandidates()
        {
            if (!string.IsNullOrWhiteSpace(Options.PublicLinkActionAssetIdParameterName))
                yield return Options.PublicLinkActionAssetIdParameterName;

            yield return "assetId";
            yield return "id";
            yield return "entityId";
        }

        private static object[]? BuildActionInvokeArgs(
            ParameterInfo[] parameters,
            string actionName,
            Dictionary<string, object> actionParameters)
        {
            if (parameters.Length == 2
                && parameters[0].ParameterType == typeof(string)
                && typeof(IDictionary<string, object>).IsAssignableFrom(parameters[1].ParameterType))
            {
                return new object[] { actionName, actionParameters };
            }

            if (parameters.Length == 3
                && parameters[0].ParameterType == typeof(string)
                && typeof(IDictionary<string, object>).IsAssignableFrom(parameters[1].ParameterType)
                && parameters[2].ParameterType == typeof(CancellationToken))
            {
                return new object[] { actionName, actionParameters, CancellationToken.None };
            }

            return null;
        }
    }
}