using ContentTransferMergeStrategy = SitecoreCommander.ContentTransfer.Model.TransferMergeStrategy;
using SitecoreCommander.ContentTransfer;
using SitecoreCommander.ContentTransfer.Model;
using SitecoreCommander.ItemTransfer;
using SitecoreCommander.ItemTransfer.Model;

namespace SitecoreCommander.Transfer
{
    internal sealed class ContentMigrationWorkflowOptions
    {
        internal TransferEnvironmentContext SourceContext { get; set; } = null!;

        internal TransferEnvironmentContext DestinationContext { get; set; } = null!;

        internal Guid TransferId { get; set; } = Guid.NewGuid();

        internal string Database { get; set; } = "master";

        internal List<DataTree> DataTrees { get; set; } = new();

        internal TimeSpan PollDelay { get; set; } = TimeSpan.FromSeconds(5);

        internal int MaxStatusPolls { get; set; } = 120;

        internal bool CleanupSourceTransfer { get; set; } = true;

        internal bool CleanupBlobSources { get; set; }

        internal bool RetryFailedItemTransfers { get; set; } = true;
    }

    internal sealed class ContentMigrationWorkflowResult
    {
        internal Guid TransferId { get; set; }

        internal List<string> ContentTransferFileNames { get; set; } = new();

        internal List<TransferStatusResult> ItemTransfers { get; set; } = new();
    }

    internal static class ContentMigrationWorkflow
    {
        internal static async Task<ContentMigrationWorkflowResult> RunAsync(
            ContentMigrationWorkflowOptions options,
            CancellationToken cancellationToken)
        {
            ValidateOptions(options);

            var request = new CreateContentTransferRequest
            {
                TransferId = options.TransferId,
                Configuration = new TransferConfiguration
                {
                    Database = options.Database,
                    DataTrees = options.DataTrees
                }
            };

            await ContentTransferClient.CreateContentTransferAsync(options.SourceContext, request, cancellationToken);
            var status = await WaitForContentTransferCompletedAsync(options, cancellationToken);
            var result = new ContentMigrationWorkflowResult { TransferId = options.TransferId };

            foreach (var chunkSet in status.ChunkSetsMetadata)
            {
                for (var chunkId = 0; chunkId < chunkSet.ChunkCount; chunkId++)
                {
                    var chunk = await ContentTransferClient.GetChunkAsync(
                        options.SourceContext,
                        options.TransferId,
                        chunkSet.ChunkSetId,
                        chunkId,
                        cancellationToken);

                    await ContentTransferClient.SaveChunkAsync(
                        options.DestinationContext,
                        options.TransferId,
                        chunkSet.ChunkSetId,
                        chunkId,
                        chunk,
                        cancellationToken);
                }

                var completed = await ContentTransferClient.CompleteChunkSetAsync(
                    options.DestinationContext,
                    options.TransferId,
                    chunkSet.ChunkSetId,
                    cancellationToken);

                if (completed == null || string.IsNullOrWhiteSpace(completed.ContentTransferFileName))
                    throw new InvalidOperationException($"Chunk set '{chunkSet.ChunkSetId}' completed without returning a .raif file name.");

                result.ContentTransferFileNames.Add(completed.ContentTransferFileName);
            }

            if (options.CleanupSourceTransfer)
                await ContentTransferClient.DeleteContentTransferAsync(options.SourceContext, options.TransferId, cancellationToken);

            foreach (var fileName in result.ContentTransferFileNames)
            {
                await WaitForBlobUploadedAsync(options, fileName, cancellationToken);
                var startResult = await ItemTransferClient.StartItemsTransferAsync(
                    options.DestinationContext,
                    options.Database,
                    blobName: fileName,
                    fileName: null,
                    cancellationToken);

                var sourceName = string.IsNullOrWhiteSpace(startResult.SourceName) ? fileName : startResult.SourceName;
                var transfer = await WaitForItemTransferFinishedAsync(options, sourceName, cancellationToken);
                result.ItemTransfers.Add(transfer);

                if (options.CleanupBlobSources)
                    await ItemTransferClient.DeleteBlobSourceAsync(options.DestinationContext, fileName, cancellationToken);
            }

            return result;
        }

        internal static DataTree CreateDataTreeFromConfig()
        {
            if (!Enum.TryParse<DataTreeScope>(Config.TransferScope, ignoreCase: true, out var scope))
                scope = DataTreeScope.SingleItem;

            if (!Enum.TryParse<ContentTransferMergeStrategy>(Config.TransferMergeStrategy, ignoreCase: true, out var mergeStrategy))
                mergeStrategy = ContentTransferMergeStrategy.KeepExistingItem;

            return new DataTree
            {
                ItemPath = Config.TransferItemPath,
                Scope = scope,
                MergeStrategy = mergeStrategy
            };
        }

        private static async Task<ContentTransferCreationStatusResponse> WaitForContentTransferCompletedAsync(
            ContentMigrationWorkflowOptions options,
            CancellationToken cancellationToken)
        {
            for (var attempt = 1; attempt <= options.MaxStatusPolls; attempt++)
            {
                var status = await ContentTransferClient.GetContentTransferStatusAsync(
                    options.SourceContext,
                    options.TransferId,
                    cancellationToken);

                if (status == null)
                    throw new InvalidOperationException($"Content transfer '{options.TransferId}' was not found.");

                if (status.State == ContentTransferState.Completed)
                    return status;

                if (status.State == ContentTransferState.Failed)
                    throw new InvalidOperationException($"Content transfer '{options.TransferId}' failed in the source environment.");

                await Task.Delay(options.PollDelay, cancellationToken);
            }

            throw new TimeoutException($"Content transfer '{options.TransferId}' did not complete after {options.MaxStatusPolls} polling attempts.");
        }

        private static async Task WaitForBlobUploadedAsync(
            ContentMigrationWorkflowOptions options,
            string blobName,
            CancellationToken cancellationToken)
        {
            for (var attempt = 1; attempt <= options.MaxStatusPolls; attempt++)
            {
                var state = await ItemTransferClient.GetBlobSourceStateAsync(options.DestinationContext, blobName, cancellationToken);
                if (state == null)
                    throw new InvalidOperationException($"Blob source '{blobName}' was not found in the destination environment.");

                if (state.BlobState == BlobState.Uploaded)
                    return;

                if (state.BlobState is BlobState.Error or BlobState.TransferredWithErrors or BlobState.Discarded)
                    throw new InvalidOperationException($"Blob source '{blobName}' is in terminal state '{state.BlobState}'. Error: {state.Error}");

                await Task.Delay(options.PollDelay, cancellationToken);
            }

            throw new TimeoutException($"Blob source '{blobName}' did not become Uploaded after {options.MaxStatusPolls} polling attempts.");
        }

        private static async Task<TransferStatusResult> WaitForItemTransferFinishedAsync(
            ContentMigrationWorkflowOptions options,
            string sourceName,
            CancellationToken cancellationToken)
        {
            for (var attempt = 1; attempt <= options.MaxStatusPolls; attempt++)
            {
                var transfer = await FindTransferBySourceNameAsync(options.DestinationContext, sourceName, cancellationToken);
                if (transfer != null)
                {
                    if (transfer.TransferState == ItemTransferState.Finished)
                        return transfer;

                    if (transfer.TransferState == ItemTransferState.Failed)
                    {
                        if (!options.RetryFailedItemTransfers)
                            throw new InvalidOperationException($"Item transfer for '{sourceName}' failed.");

                        await ItemTransferClient.RetryFailedItemsTransferAsync(
                            options.DestinationContext,
                            options.Database,
                            sourceName,
                            cancellationToken);
                    }
                }

                await Task.Delay(options.PollDelay, cancellationToken);
            }

            throw new TimeoutException($"Item transfer for '{sourceName}' did not finish after {options.MaxStatusPolls} polling attempts.");
        }

        private static async Task<TransferStatusResult?> FindTransferBySourceNameAsync(
            TransferEnvironmentContext destinationContext,
            string sourceName,
            CancellationToken cancellationToken)
        {
            var transfers = await ItemTransferClient.GetTransfersAsync(destinationContext, cancellationToken, page: 1, pageSize: 50);
            return transfers?.Transfers.FirstOrDefault(t => string.Equals(t.SourceName, sourceName, StringComparison.OrdinalIgnoreCase));
        }

        private static void ValidateOptions(ContentMigrationWorkflowOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);
            ArgumentNullException.ThrowIfNull(options.SourceContext);
            ArgumentNullException.ThrowIfNull(options.DestinationContext);

            if (string.IsNullOrWhiteSpace(options.Database))
                throw new ArgumentException("Database is required.", nameof(options));

            if (options.DataTrees.Count == 0)
                throw new ArgumentException("At least one data tree is required.", nameof(options));

            if (options.DataTrees.Any(tree => string.IsNullOrWhiteSpace(tree.ItemPath)))
                throw new ArgumentException("Every data tree must have an item path.", nameof(options));
        }
    }
}