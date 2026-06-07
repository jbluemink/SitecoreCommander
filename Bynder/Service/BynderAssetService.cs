using System.Text.Json;
using SitecoreCommander.Bynder.Model;

namespace SitecoreCommander.Bynder.Service;

/// <summary>Service for searching and retrieving assets from Bynder DAM.</summary>
public class BynderAssetService
{
    private readonly Client.BynderHttpClient _httpClient;
    private readonly string _baseDomain;

    public BynderAssetService(Auth.IBynderAuthService authService)
    {
        var authService2 = authService ?? throw new ArgumentNullException(nameof(authService));
        _httpClient = new Client.BynderHttpClient(authService2);
        _baseDomain = authService2.GetBaseDomain();
    }

    /// <summary>Search assets by name/query with pagination. Automatically retries on transient errors (503, 429, etc) with exponential backoff.</summary>
    public async Task<BynderAssetResultSet> SearchAssetsAsync(string query, int page = 1, int limit = 50)
    {
        try
        {
            var (json, totalCount) = await _httpClient.SearchAssetsAsync(query, page, limit);
            if (string.IsNullOrEmpty(json))
                return new BynderAssetResultSet { Success = false, ErrorMessage = "Empty response" };

            var results = ParseSearchResponse(json, page, limit);
            if (totalCount.HasValue)
                results.ApiTotalCount = totalCount.Value;  // Only set when header is present
            return results;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[BynderAssetService] Search error: {ex.Message}");
            return new BynderAssetResultSet { Success = false, ErrorMessage = ex.Message };
        }
    }

    /// <summary>Find single asset by exact name match.</summary>
    public async Task<BynderAsset?> FindAssetByNameAsync(string assetName)
    {
        var results = await SearchAssetsAsync(assetName, 1, 10);
        if (!results.Success || results.Assets.Count == 0)
            return null;

        var exact = results.Assets.FirstOrDefault(a => a.Name.Equals(assetName, StringComparison.OrdinalIgnoreCase));
        
        if (exact != null)
            return exact;

        // No exact match found; log partial matches for debugging
        Console.WriteLine($"[BynderAssetService] ⚠️ No exact match for '{assetName}'.");
        Console.WriteLine($"[BynderAssetService] Partial matches found ({results.Assets.Count} results):");
        foreach (var partial in results.Assets.Take(5))
        {
            Console.WriteLine($"  - {partial.Name} (ID: {partial.Id})");
        }

        // Return first partial match (best effort) but caller should handle the warning
        return results.Assets.FirstOrDefault();
    }

    /// <summary>Fetch full asset details including metadata.</summary>
    public async Task<BynderAsset?> GetAssetDetailsAsync(string assetId)
    {
        try
        {
            var doc = await _httpClient.GetAssetDetailsAsync(assetId);
            if (doc == null)
                return null;

            return ParseAssetElement(doc.RootElement);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[BynderAssetService] Error fetching {assetId}: {ex.Message}");
            return null;
        }
    }

    private BynderAssetResultSet ParseSearchResponse(string json, int page, int limit)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        var assets = new List<BynderAsset>();

        if (root.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in root.EnumerateArray())
            {
                var asset = ParseAssetElement(item);
                if (asset != null)
                    assets.Add(asset);
            }
        }

        return new BynderAssetResultSet
        {
            Assets = assets,
            Success = true,
            CurrentPage = page,
            PageSize = limit,
            TotalPages = 1,
            TotalCount = assets.Count
        };
    }

    private BynderAsset? ParseAssetElement(JsonElement element)
    {
        try
        {
            var thumbnails = ExtractRenditions(element);
            var originalUrl = FirstNonEmpty(
                GetStringProperty(element, "originalUrl"),
                GetStringProperty(element, "url"),
                GetStringProperty(element, "downloadUrl"));

            var publicUrl = FirstNonEmpty(
                GetStringProperty(element, "publicUrl"),
                GetStringProperty(element, "public_url"),
                GetStringProperty(element, "previewUrl"),
                GetStringProperty(element, "preview_url"));

            var asset = new BynderAsset
            {
                Id = GetStringProperty(element, "id") ?? string.Empty,
                Name = GetStringProperty(element, "name") ?? string.Empty,
                Description = GetStringProperty(element, "description") ?? string.Empty,
                Type = GetStringProperty(element, "type") ?? string.Empty,
                DownloadUrl = originalUrl ?? string.Empty,
                OriginalUrl = originalUrl,
                PublicUrl = publicUrl,
                ThumbnailMiniUrl = GetRendition(thumbnails, "mini"),
                ThumbnailWebimageUrl = GetRendition(thumbnails, "webimage"),
                ThumbnailSmallUrl = GetRendition(thumbnails, "small"),
                Copyright = GetStringProperty(element, "copyright"),
                Width = GetIntProperty(element, "width"),
                Height = GetIntProperty(element, "height"),
                FileSize = GetLongProperty(element, "filesize"),
                DateCreated = GetStringProperty(element, "dateCreated"),
                DateModified = GetStringProperty(element, "dateModified"),
                Thumbnails = thumbnails,
                Tags = element.TryGetProperty("tags", out var tagsElem) && tagsElem.ValueKind == JsonValueKind.Array
                    ? tagsElem.EnumerateArray().Select(t => t.GetString() ?? string.Empty).Where(t => !string.IsNullOrEmpty(t)).ToArray()
                    : Array.Empty<string>(),
                RawJson = new Dictionary<string, object> { { "raw", element.ToString() } }
            };

            return asset;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[BynderAssetService] Parse error: {ex.Message}");
            return null;
        }
    }

    private static string? GetStringProperty(JsonElement element, string propertyName)
        => element.TryGetProperty(propertyName, out var prop) && prop.ValueKind == JsonValueKind.String ? prop.GetString() : null;

    private static int? GetIntProperty(JsonElement element, string propertyName)
        => element.TryGetProperty(propertyName, out var prop) && prop.ValueKind == JsonValueKind.Number ? prop.GetInt32() : null;

    private static long? GetLongProperty(JsonElement element, string propertyName)
        => element.TryGetProperty(propertyName, out var prop) && prop.ValueKind == JsonValueKind.Number ? prop.GetInt64() : null;

    private static Dictionary<string, string> ExtractRenditions(JsonElement element)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        ExtractUrlMapFromProperty(element, "thumbnails", result);
        ExtractUrlMapFromProperty(element, "derivatives", result);
        ExtractUrlMapFromProperty(element, "renditions", result);

        return result;
    }

    private static void ExtractUrlMapFromProperty(JsonElement element, string propertyName, Dictionary<string, string> target)
    {
        if (!element.TryGetProperty(propertyName, out var mapElement))
            return;

        if (mapElement.ValueKind == JsonValueKind.Object)
        {
            foreach (var prop in mapElement.EnumerateObject())
            {
                var url = ExtractUrlValue(prop.Value);
                if (!string.IsNullOrWhiteSpace(url))
                    target[prop.Name] = url;
            }
        }
    }

    private static string? ExtractUrlValue(JsonElement value)
    {
        if (value.ValueKind == JsonValueKind.String)
            return value.GetString();

        if (value.ValueKind == JsonValueKind.Object)
        {
            var direct = FirstNonEmpty(
                GetStringProperty(value, "url"),
                GetStringProperty(value, "src"),
                GetStringProperty(value, "href"),
                GetStringProperty(value, "value"));

            if (!string.IsNullOrWhiteSpace(direct))
                return direct;
        }

        return null;
    }

    private static string? GetRendition(Dictionary<string, string> renditions, string keyContains)
    {
        foreach (var pair in renditions)
        {
            if (pair.Key.Contains(keyContains, StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(pair.Value))
                return pair.Value;
        }

        return null;
    }

    private static string? FirstNonEmpty(params string?[] candidates)
    {
        foreach (var candidate in candidates)
        {
            if (!string.IsNullOrWhiteSpace(candidate))
                return candidate;
        }

        return null;
    }
}
