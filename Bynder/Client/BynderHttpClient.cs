using System.Text.Json;

namespace SitecoreCommander.Bynder.Client;

/// <summary>Low-level HTTP client for Bynder API.</summary>
public class BynderHttpClient
{
    private readonly Auth.IBynderAuthService _authService;
    private const int MaxRetries = 3;
    private const int InitialDelayMs = 500;

    public BynderHttpClient(Auth.IBynderAuthService authService)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
    }

    /// <summary>Send authenticated GET request to Bynder API with exponential backoff retry for transient errors.</summary>
    public async Task<HttpResponseMessage> GetAsync(string url)
    {
        return await GetAsyncWithRetry(url, 0);
    }

    private async Task<HttpResponseMessage> GetAsyncWithRetry(string url, int attemptCount)
    {
        try
        {
            var client = await _authService.GetAuthenticatedClientAsync();
            var response = await client.GetAsync(url);
            
            // Check for transient errors that should trigger retry
            if (IsTransientError(response.StatusCode) && attemptCount < MaxRetries)
            {
                var delayMs = InitialDelayMs * (int)Math.Pow(2, attemptCount);
                Console.WriteLine($"[BynderHttpClient] Transient error {(int)response.StatusCode} on attempt {attemptCount + 1}/{MaxRetries}. Retrying in {delayMs}ms...");
                await Task.Delay(delayMs);
                return await GetAsyncWithRetry(url, attemptCount + 1);
            }

            return response;
        }
        catch (Exception ex)
        {
            if (IsTransientException(ex) && attemptCount < MaxRetries)
            {
                var delayMs = InitialDelayMs * (int)Math.Pow(2, attemptCount);
                Console.WriteLine($"[BynderHttpClient] Transient exception on attempt {attemptCount + 1}/{MaxRetries}: {ex.Message}. Retrying in {delayMs}ms...");
                await Task.Delay(delayMs);
                return await GetAsyncWithRetry(url, attemptCount + 1);
            }

            Console.WriteLine($"[BynderHttpClient] GET failed: {url} - {ex.Message}");
            throw;
        }
    }

    /// <summary>Check if HTTP status code is transient (should retry).</summary>
    private static bool IsTransientError(System.Net.HttpStatusCode statusCode)
    {
        return statusCode == System.Net.HttpStatusCode.ServiceUnavailable ||      // 503
               statusCode == System.Net.HttpStatusCode.RequestTimeout ||         // 408
               statusCode == System.Net.HttpStatusCode.TooManyRequests ||        // 429
               (int)statusCode == 502;                                           // Bad Gateway
    }

    /// <summary>Check if exception is transient (should retry).</summary>
    private static bool IsTransientException(Exception ex)
    {
        return ex is HttpRequestException || 
               ex is TimeoutException ||
               ex is OperationCanceledException;
    }

    /// <summary>Fetch asset details by asset ID.</summary>
    public async Task<JsonDocument?> GetAssetDetailsAsync(string assetId)
    {
        var url = $"https://{_authService.GetBaseDomain()}/api/v4/media/{assetId}/";
        try
        {
            var response = await GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            return JsonDocument.Parse(json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[BynderHttpClient] Error fetching asset {assetId}: {ex.Message}");
            return null;
        }
    }

    /// <summary>Search assets with query and pagination. Includes exponential backoff retry for transient errors. Returns (json, totalCount).</summary>
    public async Task<(string? Json, int? TotalCount)> SearchAssetsAsync(string query, int page = 1, int limit = 50)
    {
        var url = $"https://{_authService.GetBaseDomain()}/api/v4/media/?search={Uri.EscapeDataString(query)}&page={page}&limit={limit}";
        try
        {
            var response = await GetAsync(url);  // GetAsync includes exponential backoff retry
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[BynderHttpClient] Search failed with status {response.StatusCode}");
                return (null, null);
            }

            // Try read total count from response header
            int? totalCount = null;
            if (response.Headers.TryGetValues("X-Total-Count", out var totalCountValues))
            {
                if (int.TryParse(totalCountValues.FirstOrDefault(), out var parsed))
                    totalCount = parsed;
            }

            var json = await response.Content.ReadAsStringAsync();
            
            // Parse and log basic response info
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(json);
                var root = doc.RootElement;
                
                if (root.ValueKind == System.Text.Json.JsonValueKind.Array)
                {
                    var count = root.GetArrayLength();
                    if (totalCount.HasValue)
                        Console.WriteLine($"[BynderHttpClient] Search returned {count} items (total in API: {totalCount.Value})");
                    else
                        Console.WriteLine($"[BynderHttpClient] Search returned array with {count} items");
                }
                else if (root.ValueKind == System.Text.Json.JsonValueKind.Object)
                {
                    if (root.TryGetProperty("items", out var items) && items.ValueKind == System.Text.Json.JsonValueKind.Array)
                        Console.WriteLine($"[BynderHttpClient] Search returned object with {items.GetArrayLength()} items");
                    else
                        Console.WriteLine($"[BynderHttpClient] Search returned object (no 'items' array found)");
                }
            }
            catch { }

            return (json, totalCount);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[BynderHttpClient] Search error: {ex.Message}");
            return (null, null);
        }
    }
}
