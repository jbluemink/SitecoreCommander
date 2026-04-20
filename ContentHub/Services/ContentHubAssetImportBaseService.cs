using SitecoreCommander.ContentHub.Model;
using SitecoreCommander.Utils;
using Stylelabs.M.Base.Querying;
using Stylelabs.M.Base.Querying.Linq;
using Stylelabs.M.Sdk.Contracts.Base;
using Stylelabs.M.Sdk.Models.Jobs;
using Stylelabs.M.Sdk.WebClient;
using Stylelabs.M.Sdk.WebClient.Models.Upload;
using System.Text.RegularExpressions;

namespace SitecoreCommander.ContentHub.Services
{
    internal abstract class ContentHubAssetImportBaseService
    {
        private const string DefaultCultureCode = "en-US";
        private static readonly HttpClient HttpClient = new();

        protected readonly IWebMClient Client;
        protected readonly ContentHubOptions Options;

        protected ContentHubAssetImportBaseService(IWebMClient client, ContentHubOptions options)
        {
            Client = client;
            Options = options;
        }

        protected async Task SetStandardContentRepositoryAsync(IEntity entity)
        {
            var contentRepo = await Client.Entities.GetAsync(Options.StandardContentRepositoryIdentifier);
            var relation = entity.GetRelation<IChildToManyParentsRelation>(Options.ContentRepositoryRelationName);

            if (relation == null)
                return;

            if (!contentRepo.Id.HasValue)
                throw new InvalidOperationException($"Content repository '{Options.StandardContentRepositoryIdentifier}' has no id.");

            relation.Parents.Clear();
            relation.Parents.Add(contentRepo.Id.Value);
        }

        protected async Task SetApprovedLifecycleStatusAsync(IEntity entity)
        {
            var lifecycle = await Client.Entities.GetAsync(Options.ApprovedLifecycleIdentifier);
            var relation = entity.GetRelation<IChildToOneParentRelation>(Options.FinalLifecycleRelationName);

            if (relation == null)
                return;

            if (!lifecycle.Id.HasValue)
                throw new InvalidOperationException($"Lifecycle '{Options.ApprovedLifecycleIdentifier}' has no id.");

            relation.Parent = lifecycle.Id.Value;
        }

        protected async Task SetChildToOneParentRelationByNameAsync(
            IEntity entity,
            string relationName,
            string definitionName,
            string propertyName,
            string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return;

            var query = Query.CreateQuery(entities =>
                from e in entities
                where e.DefinitionName == definitionName
                select e);

            var result = await Client.Querying.QueryAsync(query);
            var relatedEntity = result.Items.FirstOrDefault(x =>
                string.Equals(GetStringPropertyValue(x, propertyName), value, StringComparison.OrdinalIgnoreCase));
            if (relatedEntity == null || !relatedEntity.Id.HasValue)
            {
                await SimpleLogger.LogAsync($"[ContentHub] taxonomy-not-found | definition={definitionName} | property={propertyName} | value={value}");
                return;
            }

            var relation = entity.GetRelation<IChildToOneParentRelation>(relationName);
            if (relation == null)
                return;

            relation.Parent = relatedEntity.Id.Value;
        }

        protected async Task SetChildToManyParentsRelationByNameAsync(
            IEntity entity,
            string relationName,
            string definitionName,
            string propertyName,
            IEnumerable<string> values)
        {
            var normalizedValues = values
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (normalizedValues.Count == 0)
                return;

            var relation = entity.GetRelation<IChildToManyParentsRelation>(relationName);
            if (relation == null)
                return;

            var query = Query.CreateQuery(entities =>
                from e in entities
                where e.DefinitionName == definitionName
                select e);

            var result = await Client.Querying.QueryAsync(query);
            var lookup = result.Items
                .Select(x => new { Entity = x, Value = GetStringPropertyValue(x, propertyName) })
                .Where(x => !string.IsNullOrWhiteSpace(x.Value) && x.Entity.Id.HasValue)
                .ToDictionary(x => x.Value!, x => x.Entity.Id!.Value, StringComparer.OrdinalIgnoreCase);

            foreach (var value in normalizedValues)
            {
                if (!lookup.TryGetValue(value, out var relatedId))
                {
                    await SimpleLogger.LogAsync($"[ContentHub] taxonomy-not-found | definition={definitionName} | property={propertyName} | value={value}");
                    continue;
                }

                if (!relation.Parents.Contains(relatedId))
                    relation.Parents.Add(relatedId);
            }
        }

        protected static void TrySetPropertyValue(IEntity entity, string propertyName, string? value)
        {
            if (string.IsNullOrWhiteSpace(propertyName) || string.IsNullOrWhiteSpace(value))
                return;

            try
            {
                // Most Content Hub properties are culture-insensitive.
                entity.SetPropertyValue(propertyName, value);
                return;
            }
            catch (Exception ex) when (ex.Message.Contains("Culture is required", StringComparison.OrdinalIgnoreCase))
            {
                // Fallback for culture-sensitive properties.
            }

            Exception? lastError = null;
            foreach (var culture in ResolveCultureCandidates(entity))
            {
                try
                {
                    entity.SetPropertyValue(propertyName, culture, value);
                    return;
                }
                catch (Exception ex) when (ex.Message.Contains("was not loaded", StringComparison.OrdinalIgnoreCase))
                {
                    lastError = ex;
                }
            }

            if (lastError != null)
                throw lastError;

            throw new InvalidOperationException($"Could not set culture-sensitive property '{propertyName}'.");
        }

        protected static string? GetStringPropertyValue(IEntity entity, string propertyName)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
                return null;

            try
            {
                return entity.GetPropertyValue<string>(propertyName);
            }
            catch (Exception ex) when (ex.Message.Contains("Culture is required", StringComparison.OrdinalIgnoreCase))
            {
                Exception? lastError = null;
                foreach (var culture in ResolveCultureCandidates(entity))
                {
                    try
                    {
                        return entity.GetPropertyValue<string>(propertyName, culture);
                    }
                    catch (Exception inner) when (inner.Message.Contains("was not loaded", StringComparison.OrdinalIgnoreCase))
                    {
                        lastError = inner;
                    }
                }

                if (lastError != null)
                    throw lastError;

                throw;
            }
        }

        private static List<System.Globalization.CultureInfo> ResolveCultureCandidates(IEntity entity)
        {
            var result = new List<System.Globalization.CultureInfo>();

            var envCulture = Environment.GetEnvironmentVariable("CONTENTHUB_CULTURE");
            if (!string.IsNullOrWhiteSpace(envCulture))
            {
                TryAddCulture(result, envCulture);
            }

            TryAddCulture(result, DefaultCultureCode);
            TryAddCulture(result, "en");

            if (entity.Cultures != null && entity.Cultures.Count > 0)
            {
                foreach (var loaded in entity.Cultures)
                {
                    if (loaded == null)
                        continue;

                    if (!result.Any(x => string.Equals(x.Name, loaded.Name, StringComparison.OrdinalIgnoreCase)))
                        result.Add(loaded);
                }
            }

            return result;
        }

        private static void TryAddCulture(List<System.Globalization.CultureInfo> list, string cultureCode)
        {
            if (string.IsNullOrWhiteSpace(cultureCode))
                return;

            try
            {
                var culture = System.Globalization.CultureInfo.GetCultureInfo(cultureCode);
                if (!list.Any(x => string.Equals(x.Name, culture.Name, StringComparison.OrdinalIgnoreCase)))
                    list.Add(culture);
            }
            catch
            {
                // ignore invalid configured culture code
            }
        }

        protected async Task CreateFetchJobAsync(long? assetId, string sourceUrl)
        {
            if (!assetId.HasValue)
                throw new ArgumentNullException(nameof(assetId));
            if (string.IsNullOrWhiteSpace(sourceUrl))
                return;

            if (!Uri.TryCreate(sourceUrl, UriKind.Absolute, out var uri))
                throw new InvalidOperationException($"Invalid source url: {sourceUrl}");

            var fetchJobRequest = new WebFetchJobRequest("File", assetId.Value);
            fetchJobRequest.Urls.Add(uri);
            var jobId = await Client.Jobs.CreateFetchJobAsync(fetchJobRequest).ConfigureAwait(false);
            await SimpleLogger.LogAsync($"[ContentHub] fetch-job-created | assetId={assetId.Value} | jobId={jobId}");
        }

        protected async Task<bool> TryUploadBinaryDirectAsync(long? assetId, string? sourceUrl, string? fileName, CancellationToken cancellationToken)
        {
            if (!Options.DirectBinaryUploadEnabled)
                return false;

            if (!assetId.HasValue || string.IsNullOrWhiteSpace(sourceUrl))
                return false;

            if (string.IsNullOrWhiteSpace(Options.UploadConfigurationName) || string.IsNullOrWhiteSpace(Options.UploadActionName))
            {
                await SimpleLogger.LogAsync("[ContentHub] direct-upload-skipped | reason=missing-upload-configuration");
                return false;
            }

            try
            {
                byte[]? bytes = null;
                string? successfulUrl = null;

                foreach (var candidateUrl in BuildSourceUrlCandidates(sourceUrl))
                    {
                        using var response = await HttpClient.GetAsync(candidateUrl, cancellationToken).ConfigureAwait(false);
                        if (!response.IsSuccessStatusCode)
                        {
                            await SimpleLogger.LogAsync($"[ContentHub] direct-upload-download-failed | assetId={assetId.Value} | status={(int)response.StatusCode} | url={candidateUrl}");
                            continue;
                        }

                        var contentType = response.Content.Headers.ContentType?.MediaType ?? string.Empty;
                        var candidateBytes = await response.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);

                        if (LooksLikeLoginOrHtmlPayload(candidateBytes, contentType))
                        {
                            await SimpleLogger.LogAsync($"[ContentHub] direct-upload-download-rejected-html | assetId={assetId.Value} | contentType={contentType} | url={candidateUrl}");
                            continue;
                        }

                        bytes = candidateBytes;
                        successfulUrl = candidateUrl;
                        break;
                    }

                if (bytes == null)
                    return false;

                if (bytes.Length == 0)
                {
                    await SimpleLogger.LogAsync($"[ContentHub] direct-upload-download-empty | assetId={assetId.Value}");
                    return false;
                }

                var safeName = string.IsNullOrWhiteSpace(fileName) ? $"asset-{assetId.Value}" : fileName.Trim();
                var uploadSource = new ByteArrayUploadSource(bytes, safeName);
                var uploadRequest = new UploadRequest(uploadSource, Options.UploadConfigurationName, Options.UploadActionName);
                uploadRequest.ActionParameters ??= new Dictionary<string, object>();
                uploadRequest.ActionParameters[Options.UploadActionAssetIdParameterName] = assetId.Value;

                await Client.Uploads.UploadAsync(uploadRequest, cancellationToken).ConfigureAwait(false);
                await SimpleLogger.LogAsync($"[ContentHub] direct-upload-succeeded | assetId={assetId.Value} | bytes={bytes.Length} | sourceUrl={successfulUrl}");
                return true;
            }
            catch (Exception ex)
            {
                var detail = ex.ToString().Replace(Environment.NewLine, " | ");
                await SimpleLogger.LogAsync($"[ContentHub] direct-upload-failed | assetId={assetId.Value} | config={Options.UploadConfigurationName} | action={Options.UploadActionName} | actionAssetParam={Options.UploadActionAssetIdParameterName} | error={ex.Message} | detail={detail}");

                if (detail.Contains("M.UploadConfiguration", StringComparison.OrdinalIgnoreCase))
                {
                    await LogAvailableUploadConfigurationsAsync();
                }

                return false;
            }
        }

        private async Task LogAvailableUploadConfigurationsAsync()
        {
            try
            {
                var query = Query.CreateQuery(entities =>
                    from e in entities
                    where e.DefinitionName == "M.UploadConfiguration"
                    select e);

                var result = await Client.Querying.QueryAsync(query).ConfigureAwait(false);
                await SimpleLogger.LogAsync($"[ContentHub] upload-configurations-found | count={result.Items.Count}");

                foreach (var item in result.Items.Take(25))
                {
                    var name = GetStringPropertyValue(item, "Name") ?? string.Empty;
                    var title = GetStringPropertyValue(item, "Title") ?? string.Empty;
                    var identifier = GetStringPropertyValue(item, "Identifier") ?? string.Empty;
                    await SimpleLogger.LogAsync($"[ContentHub] upload-configuration | id={item.Id} | Name={name} | Title={title} | Identifier={identifier}");
                }
            }
            catch (Exception ex)
            {
                await SimpleLogger.LogAsync($"[ContentHub] upload-configuration-list-failed | error={ex.Message}");
            }
        }

        private static bool LooksLikeLoginOrHtmlPayload(byte[] bytes, string contentType)
        {
            if (bytes.Length == 0)
                return false;

            if (!string.IsNullOrWhiteSpace(contentType) && contentType.Contains("text/html", StringComparison.OrdinalIgnoreCase))
                return true;

            var headLength = Math.Min(bytes.Length, 2048);
            var sample = System.Text.Encoding.UTF8.GetString(bytes, 0, headLength);
            if (string.IsNullOrWhiteSpace(sample))
                return false;

            return sample.Contains("<!DOCTYPE html", StringComparison.OrdinalIgnoreCase)
                || sample.Contains("<html", StringComparison.OrdinalIgnoreCase)
                || sample.Contains("/identity/externallogin", StringComparison.OrdinalIgnoreCase)
                || sample.Contains("SitecoreIdentityServer", StringComparison.OrdinalIgnoreCase)
                || sample.Contains("window.document.forms[0].submit", StringComparison.OrdinalIgnoreCase);
        }

        private static IEnumerable<string> BuildSourceUrlCandidates(string sourceUrl)
        {
            var urls = new List<string>();
            void Add(string? url)
            {
                if (!string.IsNullOrWhiteSpace(url) && !urls.Contains(url, StringComparer.OrdinalIgnoreCase))
                    urls.Add(url);
            }

            Add(sourceUrl);

            var queryIndex = sourceUrl.IndexOf('?', StringComparison.Ordinal);
            if (queryIndex > 0)
                Add(sourceUrl.Substring(0, queryIndex));

            if (sourceUrl.Contains("/-/media/", StringComparison.OrdinalIgnoreCase))
                Add(sourceUrl.Replace("/-/media/", "/~/media/", StringComparison.OrdinalIgnoreCase));

            if (sourceUrl.Contains("/~/media/", StringComparison.OrdinalIgnoreCase))
                Add(sourceUrl.Replace("/~/media/", "/-/media/", StringComparison.OrdinalIgnoreCase));

            if (sourceUrl.Contains("/sitecore/shell/Applications/-/media/", StringComparison.OrdinalIgnoreCase))
            {
                Add(sourceUrl.Replace("/sitecore/shell/Applications/-/media/", "/-/media/", StringComparison.OrdinalIgnoreCase));
                Add(sourceUrl.Replace("/sitecore/shell/Applications/-/media/", "/~/media/", StringComparison.OrdinalIgnoreCase));
            }

            if (sourceUrl.Contains("/sitecore/shell/Applications/~/media/", StringComparison.OrdinalIgnoreCase))
            {
                Add(sourceUrl.Replace("/sitecore/shell/Applications/~/media/", "/-/media/", StringComparison.OrdinalIgnoreCase));
                Add(sourceUrl.Replace("/sitecore/shell/Applications/~/media/", "/~/media/", StringComparison.OrdinalIgnoreCase));
            }

            var match = Regex.Match(sourceUrl, @"(?<id>[a-fA-F0-9]{32}|[a-fA-F0-9]{8}-[a-fA-F0-9]{4}-[a-fA-F0-9]{4}-[a-fA-F0-9]{4}-[a-fA-F0-9]{12})\.ashx", RegexOptions.CultureInvariant);
            if (match.Success)
            {
                var id = match.Groups["id"].Value;
                var compact = id.Replace("-", string.Empty, StringComparison.Ordinal);
                var dashed = compact.Length == 32
                    ? $"{compact.Substring(0, 8)}-{compact.Substring(8, 4)}-{compact.Substring(12, 4)}-{compact.Substring(16, 4)}-{compact.Substring(20, 12)}"
                    : id;

                Add(sourceUrl.Replace(id, compact, StringComparison.OrdinalIgnoreCase));
                Add(sourceUrl.Replace(id, dashed, StringComparison.OrdinalIgnoreCase));
                Add(sourceUrl.Replace(id, compact, StringComparison.OrdinalIgnoreCase).Replace("/-/media/", "/~/media/", StringComparison.OrdinalIgnoreCase));
                Add(sourceUrl.Replace(id, dashed, StringComparison.OrdinalIgnoreCase).Replace("/-/media/", "/~/media/", StringComparison.OrdinalIgnoreCase));
            }

            return urls;
        }
    }
}
