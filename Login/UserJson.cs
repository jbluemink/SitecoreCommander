using System.Text.Json.Serialization;

public class UserJson
{
    [JsonPropertyName("endpoints")]
    public Dictionary<string, EnvironmentConfiguration> Endpoints { get; set; }

    [JsonPropertyName("$schema")]
    public string Schema { get; set; }

    [JsonPropertyName("defaultEndpoint")]
    public string DefaultEndpoint { get; set; }
}

public class EnvironmentConfiguration
{
    [JsonPropertyName("ref")]
    public string Ref { get; set; }

    [JsonPropertyName("allowWrite")]
    public bool? AllowWrite { get; set; }

    [JsonPropertyName("host")]
    public string Host { get; set; }

    [JsonPropertyName("authority")]
    public string Authority { get; set; }

    [JsonPropertyName("useClientCredentials")]
    public bool? UseClientCredentials { get; set; }

    [JsonPropertyName("accessToken")]
    public string AccessToken { get; set; }

    [JsonPropertyName("refreshToken")]
    public string RefreshToken { get; set; }

    [JsonPropertyName("refreshTokenParameters")]
    public Dictionary<string, string> RefreshTokenParameters { get; set; }

    [JsonPropertyName("expiresIn")]
    public int? ExpiresIn { get; set; }

    [JsonPropertyName("lastUpdated")]
    public DateTime? LastUpdated { get; set; }

    [JsonPropertyName("clientId")]
    public string ClientId { get; set; } = "SitecoreCLI";

    [JsonPropertyName("clientSecret")]
    public string ClientSecret { get; set; }

    [JsonPropertyName("variables")]
    public Dictionary<string, string> Variables { get; set; }

    [JsonPropertyName("insecure")]
    public bool? Insecure { get; set; }

    [JsonPropertyName("audience")]
    public string Audience { get; set; }
}