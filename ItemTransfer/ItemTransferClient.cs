using SitecoreCommander.ItemTransfer.Model;
using SitecoreCommander.Transfer;
using System.Net.Http.Headers;
using System.Text;

namespace SitecoreCommander.ItemTransfer
{
    internal static class ItemTransferClient
    {
        internal static Task<TransfersPagedResponse?> GetTransfersAsync(
            TransferEnvironmentContext destinationContext,
            CancellationToken cancellationToken,
            int page = 1,
            int pageSize = 50)
        {
            var endpoint = $"{destinationContext.ItemTransferBaseUrl}/transfers?page={page}&pageSize={pageSize}";
            return GetJsonAsync<TransfersPagedResponse>(destinationContext, endpoint, cancellationToken);
        }

        internal static Task<TransferDetailsResult?> GetTransferByIdAsync(
            TransferEnvironmentContext destinationContext,
            string transferId,
            CancellationToken cancellationToken)
        {
            var endpoint = $"{destinationContext.ItemTransferBaseUrl}/transfers/{Escape(transferId)}";
            return GetJsonAsync<TransferDetailsResult>(destinationContext, endpoint, cancellationToken);
        }

        internal static async Task<StartItemsTransferResponse> StartItemsTransferAsync(
            TransferEnvironmentContext destinationContext,
            string databaseName,
            string? blobName,
            string? fileName,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(destinationContext);

            if (string.IsNullOrWhiteSpace(blobName) == string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("Provide either blobName or fileName, but not both.");

            var sourceQuery = !string.IsNullOrWhiteSpace(blobName)
                ? $"blobName={Escape(blobName)}"
                : $"fileName={Escape(fileName!)}";
            var endpoint = $"{destinationContext.ItemTransferBaseUrl}/transfers/databases/{Escape(databaseName)}/sources?{sourceQuery}";

            using var client = CreateClient(destinationContext);
            using var response = await client.PostAsync(endpoint, content: null, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            ItemTransferApiResponseHelper.EnsureSuccess(response, body, endpoint);

            var location = response.Headers.Location?.ToString() ?? string.Empty;
            return new StartItemsTransferResponse
            {
                Location = location,
                SourceName = GetLastPathSegment(location)
            };
        }

        internal static async Task<RetryResult?> RetryFailedItemsTransferAsync(
            TransferEnvironmentContext destinationContext,
            string databaseName,
            string sourceName,
            CancellationToken cancellationToken)
        {
            var endpoint = $"{destinationContext.ItemTransferBaseUrl}/transfers/databases/{Escape(databaseName)}/sources/{Escape(sourceName)}";
            using var client = CreateClient(destinationContext);
            using var response = await client.PutAsync(endpoint, content: null, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            return ItemTransferApiResponseHelper.DeserializeOrThrow<RetryResult>(response, body, endpoint);
        }

        internal static Task<ListItemsResult?> GetTransferredItemsAsync(
            TransferEnvironmentContext destinationContext,
            string databaseName,
            string sourceName,
            CancellationToken cancellationToken,
            int page = 1,
            int pageSize = 50)
        {
            var endpoint = $"{destinationContext.ItemTransferBaseUrl}/transfers/databases/{Escape(databaseName)}/sources/{Escape(sourceName)}/items?page={page}&pageSize={pageSize}";
            return GetJsonAsync<ListItemsResult>(destinationContext, endpoint, cancellationToken);
        }

        internal static Task<ItemDetailsResult?> GetTransferredItemDetailsAsync(
            TransferEnvironmentContext destinationContext,
            string databaseName,
            string sourceName,
            Guid itemId,
            CancellationToken cancellationToken)
        {
            var endpoint = $"{destinationContext.ItemTransferBaseUrl}/transfers/databases/{Escape(databaseName)}/sources/{Escape(sourceName)}/items/{itemId:D}";
            return GetJsonAsync<ItemDetailsResult>(destinationContext, endpoint, cancellationToken);
        }

        internal static Task<BlobSourcesResult?> GetBlobSourcesAsync(
            TransferEnvironmentContext destinationContext,
            CancellationToken cancellationToken,
            int page = 1,
            int pageSize = 50)
        {
            var endpoint = $"{destinationContext.ItemTransferBaseUrl}/sources/blobs?page={page}&pageSize={pageSize}";
            return GetJsonAsync<BlobSourcesResult>(destinationContext, endpoint, cancellationToken);
        }

        internal static Task<BlobDetailsResult?> GetBlobSourceStateAsync(
            TransferEnvironmentContext destinationContext,
            string blobName,
            CancellationToken cancellationToken)
        {
            var endpoint = $"{destinationContext.ItemTransferBaseUrl}/sources/blobs/{Escape(blobName)}";
            return GetJsonAsync<BlobDetailsResult>(destinationContext, endpoint, cancellationToken);
        }

        internal static async Task UploadBlobSourceAsync(
            TransferEnvironmentContext destinationContext,
            string blobName,
            Stream stream,
            bool gzip,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(destinationContext);
            ArgumentNullException.ThrowIfNull(stream);

            var endpoint = $"{destinationContext.ItemTransferBaseUrl}/sources/blobs/{Escape(blobName)}?gzip={gzip.ToString().ToLowerInvariant()}";
            using var client = CreateClient(destinationContext);
            using var content = new StreamContent(stream);
            content.Headers.ContentType = new MediaTypeHeaderValue(gzip ? "application/gzip" : "application/octet-stream");
            using var response = await client.PostAsync(endpoint, content, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            ItemTransferApiResponseHelper.EnsureSuccess(response, body, endpoint);
        }

        internal static async Task DeleteBlobSourceAsync(
            TransferEnvironmentContext destinationContext,
            string blobName,
            CancellationToken cancellationToken)
        {
            var endpoint = $"{destinationContext.ItemTransferBaseUrl}/sources/blobs/{Escape(blobName)}";
            using var client = CreateClient(destinationContext);
            using var response = await client.DeleteAsync(endpoint, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            ItemTransferApiResponseHelper.EnsureSuccess(response, body, endpoint);
        }

        internal static Task<FileSourcesResult?> GetFileSourcesAsync(
            TransferEnvironmentContext destinationContext,
            CancellationToken cancellationToken,
            int page = 1,
            int pageSize = 50)
        {
            var endpoint = $"{destinationContext.ItemTransferBaseUrl}/sources/files?page={page}&pageSize={pageSize}";
            return GetJsonAsync<FileSourcesResult>(destinationContext, endpoint, cancellationToken);
        }

        internal static Task<HistoryResult?> GetTransfersHistoryAsync(
            TransferEnvironmentContext destinationContext,
            CancellationToken cancellationToken,
            int page = 1,
            int pageSize = 50)
        {
            var endpoint = $"{destinationContext.ItemTransferBaseUrl}/history?page={page}&pageSize={pageSize}";
            return GetJsonAsync<HistoryResult>(destinationContext, endpoint, cancellationToken);
        }

        internal static async Task<long> DownloadBlobSourceAsync(
            TransferEnvironmentContext destinationContext,
            string blobName,
            string outputFilePath,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(destinationContext);

            if (string.IsNullOrWhiteSpace(blobName))
                throw new ArgumentException("Blob name is required.", nameof(blobName));

            if (string.IsNullOrWhiteSpace(outputFilePath))
                throw new ArgumentException("Output file path is required.", nameof(outputFilePath));

            var escapedBlobName = Escape(blobName);
            var endpoints = new[]
            {
                $"{destinationContext.ItemTransferBaseUrl}/sources/blobs/{escapedBlobName}?download=true",
                $"{destinationContext.ItemTransferBaseUrl}/sources/blobs/{escapedBlobName}/download",
                $"{destinationContext.ItemTransferBaseUrl}/sources/files/{escapedBlobName}",
                $"{destinationContext.ItemTransferBaseUrl}/sources/files/{escapedBlobName}/download"
            };

            using var client = CreateClient(destinationContext, acceptJson: false);
            var attempts = new List<string>();

            foreach (var endpoint in endpoints)
            {
                using var response = await client.GetAsync(endpoint, cancellationToken);
                var data = await response.Content.ReadAsByteArrayAsync(cancellationToken);
                var contentType = response.Content.Headers.ContentType?.MediaType ?? string.Empty;

                if (!response.IsSuccessStatusCode)
                {
                    var bodySnippet = BuildBodySnippet(data);
                    attempts.Add($"{endpoint} -> {(int)response.StatusCode} ({response.ReasonPhrase}) {bodySnippet}");
                    continue;
                }

                // Some endpoints can return JSON metadata instead of file bytes; skip those.
                if (contentType.Contains("json", StringComparison.OrdinalIgnoreCase))
                {
                    var bodySnippet = BuildBodySnippet(data);
                    attempts.Add($"{endpoint} -> 200 JSON response (not binary): {bodySnippet}");
                    continue;
                }

                var outputDirectory = Path.GetDirectoryName(outputFilePath);
                if (!string.IsNullOrWhiteSpace(outputDirectory))
                    Directory.CreateDirectory(outputDirectory);

                await File.WriteAllBytesAsync(outputFilePath, data, cancellationToken);
                return data.LongLength;
            }

            throw new InvalidOperationException(
                "Unable to download blob source content with the known Item Transfer endpoints. " +
                $"Blob: '{blobName}'. Tried endpoints: {string.Join(" | ", attempts)}");
        }

        private static async Task<T?> GetJsonAsync<T>(TransferEnvironmentContext context, string endpoint, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(context);

            using var client = CreateClient(context);
            using var response = await client.GetAsync(endpoint, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            return ItemTransferApiResponseHelper.DeserializeOrThrow<T>(response, body, endpoint);
        }

        private static HttpClient CreateClient(TransferEnvironmentContext context, bool acceptJson = true)
        {
            var client = new HttpClient();
            if (!string.IsNullOrWhiteSpace(context.AccessToken))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);

            if (!string.IsNullOrWhiteSpace(context.ApiKey))
                client.DefaultRequestHeaders.Add("sc_apikey", context.ApiKey);

            if (acceptJson)
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return client;
        }

        private static string Escape(string value)
        {
            return Uri.EscapeDataString(value);
        }

        private static string GetLastPathSegment(string location)
        {
            if (string.IsNullOrWhiteSpace(location))
                return string.Empty;

            return location.TrimEnd('/').Split('/').LastOrDefault() ?? string.Empty;
        }

        private static string BuildBodySnippet(byte[] data)
        {
            if (data.Length == 0)
                return "<empty>";

            var text = Encoding.UTF8.GetString(data);
            if (string.IsNullOrWhiteSpace(text))
                return $"<{data.Length} bytes binary>";

            var oneLine = text.Replace("\r", " ").Replace("\n", " ").Trim();
            return oneLine.Length <= 140 ? oneLine : oneLine[..140] + "...";
        }
    }
}