# Bynder API Client Library for SitecoreCommander

Generic, reusable Bynder DAM integration for SitecoreCommander. Handles OAuth2 authentication, asset discovery, media mapping for JSS-to-AI migrations, and efficient caching.

## Structure

```
Bynder/
├── Model/
│   └── BynderModels.cs              # DTOs: BynderAsset, BynderAssetResultSet, BynderMediaMapping
├── Auth/
│   └── BynderAuthService.cs         # OAuth2 authentication with token caching
├── Client/
│   └── BynderHttpClient.cs          # Low-level HTTP operations with exponential backoff retry
├── Service/
│   ├── BynderAssetService.cs        # High-level asset discovery
│   └── BynderAssetCache.cs          # Efficient in-memory + disk cache with O(1) name/ID lookups
└── README.md                         # This file
```

## Key Classes

### BynderAssetCache (NEW - Efficient Model)
Loads all Bynder assets once, caches locally, provides O(1) lookups:

**Features:**
- Lazy-loads all assets on first use (pagination handled automatically)
- Caches in-memory with name/ID indexes (case-insensitive)
- Persists to `Logs/bynder-asset-cache.json` for fast re-initialization
- No redundant API calls for repeated searches
- Automatic fallback from disk cache to API if needed

**Usage:**
```csharp
var assetService = new BynderAssetService(authService);
var cache = new BynderAssetCache(assetService);

// Initialize (loads from cache.json or fetches from API)
await cache.InitializeAsync();

// Fast O(1) lookups
var asset = cache.FindByName("TEST");           // Exact match
var assetById = cache.FindById("EB79975F-...");        // By ID
var fallback = cache.FindByNameWithFallback("needle"); // Partial match with fallback
```

**Performance:**
- **First run**: ~2-5s (fetches all ~1000 assets from API, saves cache)
- **Subsequent runs**: <100ms (loads from cache.json)
- **Lookup**: O(1) dictionary lookup (instant, no pagination loops)

## Usage

### 1. Initialize Authentication

```csharp
using SitecoreCommander.Bynder.Auth;
using SitecoreCommander.Bynder.Service;

var httpClient = new HttpClient();
var authService = new BynderAuthService(
    httpClient,
    "your-client-id",
    "your-client-secret",
    "your-domain.bynder.com"
);
```

### 2. Create Asset Service

```csharp
var assetService = new BynderAssetService(authService);
```

### 3. Search Assets

```csharp
var results = await assetService.SearchAssetsAsync("procedure image", page: 1, limit: 50);

if (results.Success)
{
    foreach (var asset in results.Assets)
    {
        Console.WriteLine($"Asset: {asset.Name} (ID: {asset.Id})");
        Console.WriteLine($"  Download URL: {asset.DownloadUrl}");
        Console.WriteLine($"  Type: {asset.Type}");
    }
}
else
{
    Console.WriteLine($"Error: {results.ErrorMessage}");
}
```

### 4. Find Asset by Name

```csharp
var asset = await assetService.FindAssetByNameAsync("surgery-guide-001.jpg");

if (asset != null)
{
    Console.WriteLine($"Found: {asset.Name} (ID: {asset.Id})");
    Console.WriteLine($"Download at: {asset.DownloadUrl}");
}
```

### 5. Get Asset Details

```csharp
var asset = await assetService.GetAssetDetailsAsync("asset-id-from-bynder");

if (asset != null)
{
    Console.WriteLine($"Width: {asset.Width} x Height: {asset.Height}");
    Console.WriteLine($"Size: {asset.FileSize} bytes");
    Console.WriteLine($"Tags: {string.Join(", ", asset.Tags)}");
}
```

## Integration with JssXpToAi

For JSS-XM to SitecoreAI media migration:

```csharp
using SitecoreCommander.Bynder.Model;
using SitecoreCommander.Bynder.Service;

// 1. Discover media in JSS source tree
var sourceMediaGuid = "{12345678-...}";
var sourceMediaName = "test.jpg";

// 2. Find in Bynder
var bynderAsset = await assetService.FindAssetByNameAsync(sourceMediaName);

if (bynderAsset != null)
{
    // 3. Create mapping
    var mapping = new BynderMediaMapping
    {
        SourceMediaGuid = sourceMediaGuid,
        SourceMediaPath = "/sitecore/media/test.jpg",
        BynderAssetId = bynderAsset.Id,
        BynderAssetName = bynderAsset.Name,
        Status = "mapped"
    };

    // 4. Store mapping for phase 3 write pipeline
    // await SaveMappingAsync(mapping);
}
```

## Error Handling

All operations are defensive:

```csharp
var results = await assetService.SearchAssetsAsync("query");

// Check success
if (!results.Success)
{
    Console.WriteLine($"Search failed: {results.ErrorMessage}");
    return;
}

// Handle empty results
if (results.Assets.Count == 0)
{
    Console.WriteLine("No assets found");
    return;
}

// Process assets
foreach (var asset in results.Assets)
{
    // ...
}
```

## Logging

All operations log to Console with prefixes:
- `[BynderAuthService]` - Authentication operations
- `[BynderHttpClient]` - HTTP requests/responses
- `[BynderAssetService]` - High-level operations

Examples:
```
[BynderAuthService] Token request failed: 401
[BynderHttpClient] GET failed: https://... - Connection timeout
[BynderAssetService] Search error: Invalid JSON response
```

## Configuration

### Environment Variables (Optional)

Store credentials in `.env` or configuration:
```
BYNDER_CLIENT_ID=your-client-id
BYNDER_CLIENT_SECRET=your-client-secret
BYNDER_DOMAIN=your-domain.bynder.com
```

### Initialization with Env Vars

```csharp
var clientId = Environment.GetEnvironmentVariable("BYNDER_CLIENT_ID");
var clientSecret = Environment.GetEnvironmentVariable("BYNDER_CLIENT_SECRET");
var domain = Environment.GetEnvironmentVariable("BYNDER_DOMAIN");

var authService = new BynderAuthService(
    new HttpClient(),
    clientId,
    clientSecret,
    domain
);
```

## Bynder API v4 Endpoints

- **Search Assets**: `GET /api/v4/media/?search=...&page=...&limit=...`
- **Get Asset Details**: `GET /api/v4/media/{id}/`
- **Authentication**: `POST /v6/authentication/oauth2/token`

For complete Bynder API documentation:  
https://bynder.docs.apiary.io/

## Token Lifecycle

- Tokens are cached in memory
- Automatic refresh 1 minute before expiry
- No token refresh on every request (improves performance)
- Credentials never logged (security)

## Performance Notes

- Reuse same HttpClient instance across requests
- Use pagination (limit parameter) for large result sets
- Search queries are case-insensitive
- Exact name matching works best (vs substring search)

## Future Enhancements

- [x] Rate limiting with exponential backoff (implemented in BynderHttpClient)
- [x] Caching layer for search results (implemented in BynderAssetCache - O(1) lookups, disk persistence)
- [ ] Batch asset operations
- [ ] Download progress reporting
- [ ] Metadata filtering (tags, type, date)
- [ ] Bynder webhook integration
- [ ] Content Hub upload automation
- [ ] ILogger integration (DI)
