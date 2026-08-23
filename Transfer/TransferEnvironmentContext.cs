using SitecoreCommander.Login;
using SitecoreCommander.Lib;

namespace SitecoreCommander.Transfer
{
    internal enum TransferAuthMode
    {
        Auto,
        Jwt,
        UserJson,
        ApiKey
    }

    internal sealed class TransferEnvironmentContext
    {
        internal string Host { get; }

        internal string AccessToken { get; }

        internal string ApiKey { get; }

        internal string ContentTransferBaseUrl => Host;

        internal string ItemTransferBaseUrl => $"{Host}/sitecore/shell/api/v3/ItemsTransfer";

        private TransferEnvironmentContext(string host, string accessToken, string apiKey)
        {
            if (string.IsNullOrWhiteSpace(host))
                throw new ArgumentException("Host is required.", nameof(host));

            if (string.IsNullOrWhiteSpace(accessToken) && string.IsNullOrWhiteSpace(apiKey))
                throw new ArgumentException("Either access token or API key is required.");

            Host = NormalizeHost(host);
            AccessToken = accessToken;
            ApiKey = NormalizeApiKey(apiKey);
        }

        internal static async Task<TransferEnvironmentContext> CreateSourceAsync(CancellationToken cancellationToken = default)
        {
            _ = cancellationToken;
            return await CreateAsync(
                Config.TransferSourceHost,
                Config.TransferSourceAuthMode,
                Config.TransferSourceJwtClientId,
                Config.TransferSourceJwtClientSecret,
                Config.TransferSourceUserJsonEnvironmentName,
                Config.TransferSourceApiKey);
        }

        internal static async Task<TransferEnvironmentContext> CreateDestinationAsync(CancellationToken cancellationToken = default)
        {
            _ = cancellationToken;
            return await CreateAsync(
                Config.TransferDestinationHost,
                Config.TransferDestinationAuthMode,
                Config.TransferDestinationJwtClientId,
                Config.TransferDestinationJwtClientSecret,
                Config.TransferDestinationUserJsonEnvironmentName,
                Config.TransferDestinationApiKey);
        }

        internal static TransferEnvironmentContext FromAccessToken(string host, string accessToken)
        {
            return new TransferEnvironmentContext(host, accessToken, string.Empty);
        }

        internal static TransferEnvironmentContext FromApiKey(string host, string apiKey)
        {
            return new TransferEnvironmentContext(host, string.Empty, apiKey);
        }

        private static async Task<TransferEnvironmentContext> CreateAsync(
            string host,
            string authMode,
            string clientId,
            string clientSecret,
            string userJsonEnvironmentName,
            string apiKey)
        {
            if (!Enum.TryParse<TransferAuthMode>(authMode, ignoreCase: true, out var requestedMode))
                requestedMode = TransferAuthMode.Auto;

            var mode = ResolveMode(requestedMode, host, clientId, clientSecret, apiKey);

            if (mode == TransferAuthMode.UserJson)
                return CreateFromUserJson(host, userJsonEnvironmentName);

            if (string.IsNullOrWhiteSpace(host))
                throw new InvalidOperationException("Transfer host is not configured.");

            if (mode == TransferAuthMode.ApiKey)
            {
                if (string.IsNullOrWhiteSpace(apiKey))
                    throw new InvalidOperationException("Transfer API key is required when auth mode is ApiKey.");

                return new TransferEnvironmentContext(host, string.Empty, apiKey);
            }

            if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
                throw new InvalidOperationException("Transfer JWT client id and client secret are required.");

            var token = await SitecoreJwtClient.GetJwtAsync(clientId, clientSecret, "https://api.sitecorecloud.io");
            if (token == null || string.IsNullOrWhiteSpace(token.access_token))
                throw new InvalidOperationException("Failed to obtain a transfer JWT token.");

            return new TransferEnvironmentContext(host, token.access_token, apiKey);
        }

        private static TransferEnvironmentContext CreateFromUserJson(string host, string endpointName)
        {
            if (string.IsNullOrWhiteSpace(Config.XMCloudUserJsonPath) || !File.Exists(Config.XMCloudUserJsonPath))
                throw new InvalidOperationException("XMCloud user.json path is not configured or file does not exist.");

            var environment = string.IsNullOrWhiteSpace(endpointName)
                ? SitecoreCommander.Lib.Login.GetSitecoreEnvironment()
                : SitecoreCommander.Lib.Login.GetSitecoreEnvironment(endpointName);

            var resolvedHost = string.IsNullOrWhiteSpace(host) ? environment.Host : host;
            if (string.IsNullOrWhiteSpace(resolvedHost))
                throw new InvalidOperationException("Transfer host is not configured and could not be resolved from user.json.");

            if (string.IsNullOrWhiteSpace(environment.AccessToken))
                throw new InvalidOperationException("user.json endpoint does not contain an access token. Re-authenticate with Sitecore CLI.");

            return new TransferEnvironmentContext(resolvedHost, environment.AccessToken, string.Empty);
        }

        private static TransferAuthMode ResolveMode(
            TransferAuthMode requestedMode,
            string host,
            string clientId,
            string clientSecret,
            string apiKey)
        {
            if (requestedMode != TransferAuthMode.Auto)
                return requestedMode;

            // Prefer API key by default for localhost CM, then JWT for cloud automation.
            if (IsLocalHost(host) && !string.IsNullOrWhiteSpace(apiKey))
                return TransferAuthMode.ApiKey;

            if (!string.IsNullOrWhiteSpace(clientId) && !string.IsNullOrWhiteSpace(clientSecret))
                return TransferAuthMode.Jwt;

            if (!string.IsNullOrWhiteSpace(Config.XMCloudUserJsonPath) && File.Exists(Config.XMCloudUserJsonPath))
                return TransferAuthMode.UserJson;

            if (!string.IsNullOrWhiteSpace(apiKey))
                return TransferAuthMode.ApiKey;

            return TransferAuthMode.Jwt;
        }

        private static bool IsLocalHost(string host)
        {
            if (string.IsNullOrWhiteSpace(host))
                return false;

            return host.Contains("localhost", StringComparison.OrdinalIgnoreCase) ||
                   host.Contains("127.0.0.1", StringComparison.OrdinalIgnoreCase);
        }

        private static string NormalizeApiKey(string apiKey)
        {
            return (apiKey ?? string.Empty).Trim().Trim('{', '}');
        }

        private static string NormalizeHost(string host)
        {
            var normalized = host.Trim().TrimEnd('/');
            if (!normalized.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !normalized.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                normalized = "https://" + normalized;
            }

            return normalized;
        }
    }
}