using System.Text.Json;
using System.Net.Http.Headers;

namespace SitecoreCommander.Bynder.Auth;

public interface IBynderAuthService
{
    Task<string?> GetAccessTokenAsync();
    Task<HttpClient> GetAuthenticatedClientAsync();
    string GetBaseDomain();
}

/// <summary>Direct Bearer-token authentication for Bynder API.</summary>
public class BynderBearerAuthService : IBynderAuthService
{
    private readonly string _accessToken;
    private readonly string _baseDomain;

    public BynderBearerAuthService(string accessToken, string baseDomain)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
            throw new ArgumentNullException(nameof(accessToken));

        _accessToken = accessToken.Trim();
        if (_accessToken.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            _accessToken = _accessToken.Substring("Bearer ".Length).Trim();

        _baseDomain = (baseDomain ?? throw new ArgumentNullException(nameof(baseDomain)))
            .Replace("https://", string.Empty)
            .Replace("http://", string.Empty)
            .TrimEnd('/');
    }

    public string GetBaseDomain() => _baseDomain;

    public Task<string?> GetAccessTokenAsync()
        => Task.FromResult<string?>(_accessToken);

    public Task<HttpClient> GetAuthenticatedClientAsync()
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
        return Task.FromResult(client);
    }
}

/// <summary>OAuth2 Client Credentials authentication for Bynder API.</summary>
public class BynderAuthService : IBynderAuthService
{
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly string _baseDomain;
    private readonly HttpClient _httpClient;
    private string? _cachedToken;
    private DateTimeOffset _tokenExpiry;

    public BynderAuthService(HttpClient httpClient, string clientId, string clientSecret, string baseDomain)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _clientId = clientId ?? throw new ArgumentNullException(nameof(clientId));
        _clientSecret = clientSecret ?? throw new ArgumentNullException(nameof(clientSecret));
        _baseDomain = (baseDomain ?? throw new ArgumentNullException(nameof(baseDomain)))
            .Replace("https://", string.Empty)
            .Replace("http://", string.Empty)
            .TrimEnd('/');
        _tokenExpiry = DateTimeOffset.MinValue;
    }

    public string GetBaseDomain() => _baseDomain;

    public async Task<string?> GetAccessTokenAsync()
    {
        // Return cached token if valid (with 1-minute buffer)
        if (!string.IsNullOrEmpty(_cachedToken) && DateTimeOffset.UtcNow.AddMinutes(1) < _tokenExpiry)
            return _cachedToken;

        try
        {
            var tokenUrl = $"https://{_baseDomain}/v6/authentication/oauth2/token";
            var form = new Dictionary<string, string>
            {
                { "client_id", _clientId },
                { "client_secret", _clientSecret },
                { "grant_type", "client_credentials" }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, tokenUrl)
            {
                Content = new FormUrlEncodedContent(form)
            };

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[BynderAuthService] Token request failed: {response.StatusCode}");
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (!root.TryGetProperty("access_token", out var tokenElem) ||
                !root.TryGetProperty("expires_in", out var expiresElem))
                return null;

            _cachedToken = tokenElem.GetString();
            var expiresIn = expiresElem.GetInt32();
            _tokenExpiry = DateTimeOffset.UtcNow.AddSeconds(expiresIn);

            return _cachedToken;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[BynderAuthService] Error: {ex.Message}");
            return null;
        }
    }

    public async Task<HttpClient> GetAuthenticatedClientAsync()
    {
        var token = await GetAccessTokenAsync();
        if (string.IsNullOrEmpty(token))
            throw new InvalidOperationException("Failed to obtain Bynder access token.");

        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        return client;
    }
}
