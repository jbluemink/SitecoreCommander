using SitecoreCommander.ContentTransfer.Model;
using SitecoreCommander.Transfer;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace SitecoreCommander.ContentTransfer
{
    internal static class ContentTransferClient
    {
        internal static async Task CreateContentTransferAsync(
            TransferEnvironmentContext sourceContext,
            CreateContentTransferRequest request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(sourceContext);
            ArgumentNullException.ThrowIfNull(request);

            var endpoint = $"{sourceContext.ContentTransferBaseUrl}/sitecore/api/content/transfer/v1/transfers";
            using var client = CreateClient(sourceContext);
            var json = JsonSerializer.Serialize(request, ContentTransferApiResponseHelper.JsonOptions);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            using var response = await client.PostAsync(endpoint, content, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            ContentTransferApiResponseHelper.EnsureSuccess(response, body, endpoint);
        }

        internal static async Task<ContentTransferCreationStatusResponse?> GetContentTransferStatusAsync(
            TransferEnvironmentContext sourceContext,
            Guid transferId,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(sourceContext);

            var endpoint = $"{sourceContext.ContentTransferBaseUrl}/sitecore/api/content/transfer/v1/transfers/{transferId:D}/status";
            using var client = CreateClient(sourceContext);
            using var response = await client.GetAsync(endpoint, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            return ContentTransferApiResponseHelper.DeserializeOrThrow<ContentTransferCreationStatusResponse>(response, body, endpoint);
        }

        internal static async Task<ContentTransferChunk> GetChunkAsync(
            TransferEnvironmentContext sourceContext,
            Guid transferId,
            Guid chunkSetId,
            int chunkId,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(sourceContext);

            var endpoint = $"{sourceContext.ContentTransferBaseUrl}/sitecore/api/content/transfer/v1/transfers/{transferId:D}/chunksets/{chunkSetId:D}/chunks/{chunkId}";
            using var client = CreateClient(sourceContext);
            using var response = await client.GetAsync(endpoint, cancellationToken);
            var data = await response.Content.ReadAsByteArrayAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var body = Encoding.UTF8.GetString(data);
                ContentTransferApiResponseHelper.ThrowRequestFailed(response, body, endpoint);
            }

            return new ContentTransferChunk
            {
                Data = data,
                IsMedia = TryGetContentDispositionParameter(response.Content.Headers.ContentDisposition, "IsMedia", out var isMedia) &&
                    bool.TryParse(isMedia, out var parsedIsMedia) && parsedIsMedia,
                ItemsProcessed = TryGetContentDispositionParameter(response.Content.Headers.ContentDisposition, "ItemsProcessed", out var itemsProcessed) &&
                    int.TryParse(itemsProcessed, out var parsedItemsProcessed) ? parsedItemsProcessed : null,
                ItemsSkipped = TryGetContentDispositionParameter(response.Content.Headers.ContentDisposition, "ItemsSkipped", out var itemsSkipped) &&
                    int.TryParse(itemsSkipped, out var parsedItemsSkipped) ? parsedItemsSkipped : null
            };
        }

        internal static async Task SaveChunkAsync(
            TransferEnvironmentContext destinationContext,
            Guid transferId,
            Guid chunkSetId,
            int chunkId,
            ContentTransferChunk chunk,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(destinationContext);
            ArgumentNullException.ThrowIfNull(chunk);

            var endpoint = $"{destinationContext.ContentTransferBaseUrl}/sitecore/api/content/transfer/v1/transfers/{transferId:D}/chunksets/{chunkSetId:D}/chunks/{chunkId}?isMedia={chunk.IsMedia.ToString().ToLowerInvariant()}";
            using var client = CreateClient(destinationContext);
            using var content = new ByteArrayContent(chunk.Data);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            using var response = await client.PutAsync(endpoint, content, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            ContentTransferApiResponseHelper.EnsureSuccess(response, body, endpoint);
        }

        internal static async Task<ChunkSetCompleteResponse?> CompleteChunkSetAsync(
            TransferEnvironmentContext destinationContext,
            Guid transferId,
            Guid chunkSetId,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(destinationContext);

            var endpoint = $"{destinationContext.ContentTransferBaseUrl}/sitecore/api/content/transfer/v1/transfers/{transferId:D}/chunksets/{chunkSetId:D}/complete";
            using var client = CreateClient(destinationContext);
            using var response = await client.PostAsync(endpoint, content: null, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            return ContentTransferApiResponseHelper.DeserializeOrThrow<ChunkSetCompleteResponse>(response, body, endpoint);
        }

        internal static async Task DeleteContentTransferAsync(
            TransferEnvironmentContext sourceContext,
            Guid transferId,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(sourceContext);

            var endpoint = $"{sourceContext.ContentTransferBaseUrl}/sitecore/api/content/transfer/v1/transfers/{transferId:D}";
            using var client = CreateClient(sourceContext);
            using var response = await client.DeleteAsync(endpoint, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            ContentTransferApiResponseHelper.EnsureSuccess(response, body, endpoint);
        }

        private static HttpClient CreateClient(TransferEnvironmentContext context)
        {
            var client = new HttpClient();
            if (!string.IsNullOrWhiteSpace(context.AccessToken))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);

            if (!string.IsNullOrWhiteSpace(context.ApiKey))
                client.DefaultRequestHeaders.Add("sc_apikey", context.ApiKey);

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return client;
        }

        private static bool TryGetContentDispositionParameter(ContentDispositionHeaderValue? contentDisposition, string name, out string value)
        {
            value = string.Empty;
            if (contentDisposition == null)
                return false;

            var parameter = contentDisposition.Parameters.FirstOrDefault(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));
            if (parameter == null || string.IsNullOrWhiteSpace(parameter.Value))
                return false;

            value = parameter.Value.Trim('"');
            return true;
        }
    }
}