using System.Text.Json;

public static class JwtTokenHelper
{
    public static string? GetTenantNameFromJwt(string jwtToken)
    {
        if (string.IsNullOrWhiteSpace(jwtToken))
            return null;

        var parts = jwtToken.Split('.');
        if (parts.Length < 2)
            return null;

        // JWT payload is second part, base64url encoded
        string payload = parts[1];
        // Base64url to  base64
        payload = payload.Replace('-', '+').Replace('_', '/');
        switch (payload.Length % 4)
        {
            case 2: payload += "=="; break;
            case 3: payload += "="; break;
        }

        try
        {
            var json = Convert.FromBase64String(payload);
            var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("https://auth.sitecorecloud.io/claims/tenant_name", out var tenantName))
            {
                return tenantName.GetString();
            }
        }
        catch
        {
            // error while decoding
            return null;
        }

        return null;
    }

    public static string GetInstanceUrlFromJwt(string accessToken)
    {
        string tenant = GetTenantNameFromJwt(accessToken);
        return $"https://xmc-{tenant}.sitecorecloud.io/";
    }
}
