using System.Collections.Concurrent;
using System.Text.Json;
using SitecoreCommander.Bynder.Model;

namespace SitecoreCommander.Bynder.Service;

/// <summary>
/// Efficient in-memory cache for Bynder assets with local JSON persistence.
/// Loads all assets once, builds name/ID indexes, and provides fast O(1) lookups.
/// </summary>
public class BynderAssetCache
{
    private readonly BynderAssetService _assetService;
    private readonly string _cacheFilePath;
    
    private ConcurrentDictionary<string, BynderAsset>? _assetsByIdCache;
    private ConcurrentDictionary<string, BynderAsset>? _assetsByNameCache;
    private bool _isInitialized;

    public BynderAssetCache(BynderAssetService assetService, string? cacheFilePath = null)
    {
        _assetService = assetService ?? throw new ArgumentNullException(nameof(assetService));
        
        // Default cache path: Logs/bynder-asset-cache.json
        _cacheFilePath = cacheFilePath ?? Path.Combine("Logs", "bynder-asset-cache.json");
        _isInitialized = false;
    }

    /// <summary>Initialize cache: load from disk if exists, otherwise fetch from API and save.</summary>
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (_isInitialized)
            return; // Already loaded

        _assetsByIdCache = new ConcurrentDictionary<string, BynderAsset>();
        _assetsByNameCache = new ConcurrentDictionary<string, BynderAsset>(StringComparer.OrdinalIgnoreCase);

        // Try load from cache file first
        if (File.Exists(_cacheFilePath))
        {
            Console.WriteLine($"[BynderAssetCache] Loading cache from disk: {_cacheFilePath}");
            try
            {
                await LoadFromCacheFileAsync();
                Console.WriteLine($"[BynderAssetCache] ✓ Loaded {_assetsByIdCache.Count} assets from cache file");
                _isInitialized = true;
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BynderAssetCache] Warning: Failed to load cache file ({ex.Message}). Fetching from API...");
            }
        }

        // Fetch from API and build cache
        Console.WriteLine("[BynderAssetCache] Fetching all assets from Bynder API...");
        await FetchAllAssetsFromApiAsync(cancellationToken);
        
        // Save to cache file for next time
        await SaveCacheFileAsync();
        
        Console.WriteLine($"[BynderAssetCache] ✓ Cached {_assetsByIdCache.Count} assets locally");
        _isInitialized = true;
    }

    /// <summary>Find asset by exact name match (case-insensitive).</summary>
    public BynderAsset? FindByName(string assetName)
    {
        EnsureInitialized();
        
        if (_assetsByNameCache!.TryGetValue(assetName, out var asset))
        {
            return asset;
        }

        return null;
    }

    /// <summary>Find asset by ID.</summary>
    public BynderAsset? FindById(string assetId)
    {
        EnsureInitialized();
        
        if (_assetsByIdCache!.TryGetValue(assetId, out var asset))
        {
            return asset;
        }

        return null;
    }

    /// <summary>Find asset by name, with fallback to partial name matching.</summary>
    public BynderAsset? FindByNameWithFallback(string assetName, int maxPartialMatches = 5)
    {
        EnsureInitialized();

        // Try exact match first
        if (_assetsByNameCache!.TryGetValue(assetName, out var exact))
        {
            return exact;
        }

        // Fallback: find partial matches (substring)
        var lowerName = assetName.ToLowerInvariant();
        var partialMatches = _assetsByNameCache.Values
            .Where(a => a.Name.ToLowerInvariant().Contains(lowerName))
            .Take(maxPartialMatches)
            .ToList();

        if (partialMatches.Count > 0)
        {
            Console.WriteLine($"[BynderAssetCache] ⚠️  No exact match for '{assetName}'. Found {partialMatches.Count} partial matches:");
            foreach (var match in partialMatches)
            {
                Console.WriteLine($"  - {match.Name} (ID: {match.Id})");
            }
            return partialMatches.First(); // Return best guess
        }

        return null;
    }

    /// <summary>Get all cached assets.</summary>
    public IEnumerable<BynderAsset> GetAllAssets()
    {
        EnsureInitialized();
        return _assetsByIdCache!.Values;
    }

    /// <summary>Get count of cached assets.</summary>
    public int GetAssetCount()
    {
        EnsureInitialized();
        return _assetsByIdCache!.Count;
    }

    /// <summary>Clear cache (for testing or manual refresh).</summary>
    public void Clear()
    {
        _assetsByIdCache?.Clear();
        _assetsByNameCache?.Clear();
        _isInitialized = false;
    }

    /// <summary>Refresh cache from API (download all assets fresh).</summary>
    public async Task RefreshFromApiAsync(CancellationToken cancellationToken = default)
    {
        Console.WriteLine("[BynderAssetCache] Refreshing cache from API...");
        Clear();
        
        _assetsByIdCache = new ConcurrentDictionary<string, BynderAsset>();
        _assetsByNameCache = new ConcurrentDictionary<string, BynderAsset>(StringComparer.OrdinalIgnoreCase);

        await FetchAllAssetsFromApiAsync(cancellationToken);
        await SaveCacheFileAsync();
        
        _isInitialized = true;
        Console.WriteLine($"[BynderAssetCache] ✓ Refreshed cache: {_assetsByIdCache.Count} assets");
    }

    // Private helpers

    private void EnsureInitialized()
    {
        if (!_isInitialized || _assetsByIdCache == null || _assetsByNameCache == null)
        {
            throw new InvalidOperationException("Cache not initialized. Call InitializeAsync() first.");
        }
    }

    private async Task FetchAllAssetsFromApiAsync(CancellationToken cancellationToken)
    {
        int page = 1;
        int totalFetched = 0;
        int? totalInApi = null;
        const int limit = 100;
        const int saveEveryNPages = 50; // Save incrementally every 50 pages (~5000 assets)
        var startTime = DateTime.UtcNow;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Bynder API: pages start at 1
            var results = await _assetService.SearchAssetsAsync("*", page, limit);
            
            if (!results.Success || results.Assets.Count == 0)
            {
                Console.WriteLine($"[BynderAssetCache] Reached end of results at page {page}");
                break;
            }

            // Capture total count from API on first page (via X-Total-Count header)
            // Capture total count only from API header (ApiTotalCount), not from page count
            if (page == 1 && results.ApiTotalCount.HasValue && results.ApiTotalCount.Value > 0)
                totalInApi = results.ApiTotalCount.Value;

            foreach (var asset in results.Assets)
            {
                _assetsByIdCache!.TryAdd(asset.Id, asset);
                _assetsByNameCache!.TryAdd(asset.Name, asset);
                totalFetched++;
            }

            // Log progress with estimate if total is known
            if (totalInApi.HasValue && totalInApi.Value > 0)
            {
                var pct = (double)totalFetched / totalInApi.Value * 100;
                var elapsed = DateTime.UtcNow - startTime;
                var estimatedTotal = elapsed.TotalSeconds / (totalFetched / (double)totalInApi.Value);
                var remaining = TimeSpan.FromSeconds(estimatedTotal - elapsed.TotalSeconds);
                Console.WriteLine($"[BynderAssetCache] Page {page}: {totalFetched}/{totalInApi.Value} ({pct:F0}%) — ~{remaining.TotalSeconds:F0}s remaining");
            }
            else
            {
                 Console.WriteLine($"[BynderAssetCache] Page {page}: {results.Assets.Count} assets fetched (running total: {totalFetched})");
            }

            // Save incrementally every N pages so progress is not lost on interruption
            if (page % saveEveryNPages == 0)
            {
                Console.WriteLine($"[BynderAssetCache] Saving checkpoint at page {page} ({totalFetched} assets)...");
                await SaveCacheFileAsync();
            }

            // If we got fewer than limit, this was the last page
            if (results.Assets.Count < limit)
                break;

            page++;
        }

        var duration = DateTime.UtcNow - startTime;
        Console.WriteLine($"[BynderAssetCache] Done: {totalFetched} assets in {duration.TotalSeconds:F1}s");
    }

    private async Task LoadFromCacheFileAsync()
    {
        var json = await File.ReadAllTextAsync(_cacheFilePath);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        if (root.ValueKind != JsonValueKind.Array)
            throw new InvalidOperationException("Cache file is not a JSON array");

        foreach (var elem in root.EnumerateArray())
        {
            var asset = ParseAssetFromJson(elem);
            if (asset != null)
            {
                _assetsByIdCache!.TryAdd(asset.Id, asset);
                _assetsByNameCache!.TryAdd(asset.Name, asset);
            }
        }
    }

    private async Task SaveCacheFileAsync()
    {
        // Ensure directory exists
        var dir = Path.GetDirectoryName(_cacheFilePath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        var assets = _assetsByIdCache!.Values.ToList();
        var json = JsonSerializer.Serialize(assets, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(_cacheFilePath, json);
        Console.WriteLine($"[BynderAssetCache] Saved cache to: {_cacheFilePath}");
    }

    private static BynderAsset? ParseAssetFromJson(JsonElement elem)
    {
        try
        {
            return JsonSerializer.Deserialize<BynderAsset>(elem.GetRawText());
        }
        catch
        {
            return null;
        }
    }
}
