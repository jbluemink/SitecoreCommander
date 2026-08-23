using SitecoreCommander.Login;

namespace SitecoreCommander.Transfer
{
    internal sealed class TransferEnvironmentContext
    {
        internal string Host { get; }

        internal string AccessToken { get; }

        internal string ContentTransferBaseUrl => Host;

        internal string ItemTransferBaseUrl => $"{Host}/sitecore/shell/api/v3/ItemsTransfer";

        private TransferEnvironmentContext(string host, string accessToken)
        {
            if (string.IsNullOrWhiteSpace(host))
                throw new ArgumentException("Host is required.", nameof(host));

            if (string.IsNullOrWhiteSpace(accessToken))
                throw new ArgumentException("Access token is required.", nameof(accessToken));

            Host = NormalizeHost(host);
            AccessToken = accessToken;
        }

        internal static async Task<TransferEnvironmentContext> CreateSourceAsync(CancellationToken cancellationToken = default)
        {
            _ = cancellationToken;
            return await CreateAsync(
                Config.TransferSourceHost,
                Config.TransferSourceJwtClientId,
                Config.TransferSourceJwtClientSecret);
        }

        internal static async Task<TransferEnvironmentContext> CreateDestinationAsync(CancellationToken cancellationToken = default)
        {
            _ = cancellationToken;
            return await CreateAsync(
                Config.TransferDestinationHost,
                Config.TransferDestinationJwtClientId,
                Config.TransferDestinationJwtClientSecret);
        }

        internal static TransferEnvironmentContext FromAccessToken(string host, string accessToken)
        {
            return new TransferEnvironmentContext(host, accessToken);
        }

        private static async Task<TransferEnvironmentContext> CreateAsync(string host, string clientId, string clientSecret)
        {
            if (string.IsNullOrWhiteSpace(host))
                throw new InvalidOperationException("Transfer host is not configured.");

            if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
                throw new InvalidOperationException("Transfer JWT client id and client secret are required.");

            var token = await SitecoreJwtClient.GetJwtAsync(clientId, clientSecret, "https://api.sitecorecloud.io");
            if (token == null || string.IsNullOrWhiteSpace(token.access_token))
                throw new InvalidOperationException("Failed to obtain a transfer JWT token.");

            return new TransferEnvironmentContext(host, token.access_token);
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