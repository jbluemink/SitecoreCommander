using SitecoreCommander.ContentTransfer.Model;
using SitecoreCommander.ContentTransfer;
using SitecoreCommander.ItemTransfer;
using SitecoreCommander.ItemTransfer.Model;
using SitecoreCommander.Transfer;
using System.Text.Json;

namespace SitecoreCommander.Examples
{
    public class TransferApiExamples : ExampleBase
    {
        public override async Task<bool> InitializeAsync()
        {
            await VerificationHelper.LogAsync("Transfer API uses source/destination automation client settings.", ConsoleColor.Gray);
            return true;
        }

        public override async Task RunAsync()
        {
            VerificationHelper.PrintSectionHeader("SitecoreAI Content Transfer / Item Transfer API Examples");

            await RunExampleAsync(
                "List Blob Sources",
                "Read available .raif blobs in the destination environment.",
                ListBlobSourcesExample);

            await RunExampleAsync(
                "List Item Transfers",
                "Read current and completed item transfers in the destination environment.",
                ListTransfersExample);

            await RunExampleAsync(
                "Transfer History",
                "Read historical item transfer events in the destination environment.",
                TransferHistoryExample);

            await RunExampleAsync(
                "Download Available RAIF Blobs To Disk",
                "Download destination .raif blob sources and store them locally.",
                DownloadAvailableRaifBlobsToDiskExample);

            await RunExampleAsync(
                "Export Content Transfer Chunks To Disk",
                "Create a source content transfer and export all binary chunks locally for archival/debugging.",
                ExportContentTransferChunksToDiskExample);

            await RunExampleAsync(
                "End-to-End Content Migration",
                "Create a content transfer, copy chunks, consume the .raif file, and monitor completion.",
                EndToEndTransferExample);
        }

        private async Task ListBlobSourcesExample()
        {
            var destination = await TransferEnvironmentContext.CreateDestinationAsync();
            var result = await ItemTransferClient.GetBlobSourcesAsync(destination, CancellationToken.None);

            var verification = await VerificationHelper.VerifyResponseAsync(
                result,
                "List Blob Sources",
                r => r != null);

            if (!verification.Success)
            {
                VerificationHelper.PrintFailure("List Blob Sources", verification.ErrorDetails);
                return;
            }

            VerificationHelper.PrintSuccess("List Blob Sources", $"Found {verification.Data!.TotalCount} blob source(s)");
            foreach (var source in verification.Data.Sources.Take(5))
            {
                Console.WriteLine($"   • {source.Name} ({source.BlobState})");
            }
        }

        private async Task ListTransfersExample()
        {
            var destination = await TransferEnvironmentContext.CreateDestinationAsync();
            var result = await ItemTransferClient.GetTransfersAsync(destination, CancellationToken.None);

            var verification = await VerificationHelper.VerifyResponseAsync(
                result,
                "List Item Transfers",
                r => r != null);

            if (!verification.Success)
            {
                VerificationHelper.PrintFailure("List Item Transfers", verification.ErrorDetails);
                return;
            }

            VerificationHelper.PrintSuccess("List Item Transfers", $"Found {verification.Data!.TotalCount} transfer(s)");
            foreach (var transfer in verification.Data.Transfers.Take(5))
            {
                Console.WriteLine($"   • {transfer.SourceName} ({transfer.TransferState}) [{transfer.DatabaseName}]");
            }
        }

        private async Task TransferHistoryExample()
        {
            var destination = await TransferEnvironmentContext.CreateDestinationAsync();
            var result = await ItemTransferClient.GetTransfersHistoryAsync(destination, CancellationToken.None);

            var verification = await VerificationHelper.VerifyResponseAsync(
                result,
                "Transfer History",
                r => r != null);

            if (!verification.Success)
            {
                VerificationHelper.PrintFailure("Transfer History", verification.ErrorDetails);
                return;
            }

            VerificationHelper.PrintSuccess("Transfer History", $"Found {verification.Data!.TotalCount} history record(s)");
            foreach (var history in verification.Data.Sources.Take(5))
            {
                Console.WriteLine($"   • {history.SourceName} ({history.Events.Count} event(s))");
            }
        }

        private async Task EndToEndTransferExample()
        {
            if (string.IsNullOrWhiteSpace(Config.TransferItemPath))
            {
                await VerificationHelper.LogAsync("TransferItemPath is empty; skipping end-to-end migration example.", ConsoleColor.Yellow);
                return;
            }

            var dataTree = ContentMigrationWorkflow.CreateDataTreeFromConfig();
            PrintTransferSummary(dataTree);

            if (dataTree.MergeStrategy == SitecoreCommander.ContentTransfer.Model.TransferMergeStrategy.OverrideExistingTree)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("OverrideExistingTree can delete an existing destination item tree before writing transferred items.");
                Console.ResetColor();
            }

            if (!VerificationHelper.PromptConfirmation("Start this content migration now?"))
                return;

            var source = await TransferEnvironmentContext.CreateSourceAsync();
            var destination = await TransferEnvironmentContext.CreateDestinationAsync();

            var result = await ContentMigrationWorkflow.RunAsync(
                new ContentMigrationWorkflowOptions
                {
                    SourceContext = source,
                    DestinationContext = destination,
                    Database = Config.TransferDatabase,
                    DataTrees = new List<DataTree> { dataTree },
                    CleanupSourceTransfer = true,
                    CleanupBlobSources = false
                },
                CancellationToken.None);

            VerificationHelper.PrintSuccess("End-to-End Content Migration", $"Transfer {result.TransferId} completed");
            foreach (var fileName in result.ContentTransferFileNames)
            {
                Console.WriteLine($"   • RAIF: {fileName}");
            }
        }

        private async Task DownloadAvailableRaifBlobsToDiskExample()
        {
            var destination = await TransferEnvironmentContext.CreateDestinationAsync();
            var result = await ItemTransferClient.GetBlobSourcesAsync(destination, CancellationToken.None);

            var verification = await VerificationHelper.VerifyResponseAsync(
                result,
                "Download Available RAIF Blobs To Disk",
                r => r != null);

            if (!verification.Success)
            {
                VerificationHelper.PrintFailure("Download Available RAIF Blobs To Disk", verification.ErrorDetails);
                return;
            }

            var downloadableStates = new[]
            {
                BlobState.Uploaded
            };

            var candidates = verification.Data!.Sources
                .Where(s => downloadableStates.Contains(s.BlobState))
                .ToList();

            if (candidates.Count == 0)
            {
                var stateSummary = string.Join(", ",
                    verification.Data.Sources
                        .GroupBy(s => s.BlobState)
                        .Select(g => $"{g.Key}={g.Count()}"));

                await VerificationHelper.LogAsync(
                    "No .raif blob sources in 'Uploaded' state were found. " +
                    "Only Uploaded blobs are candidates for direct consumption/download. " +
                    $"Current states: {stateSummary}",
                    ConsoleColor.Yellow);
                return;
            }

            Console.WriteLine($"Found {candidates.Count} downloadable blob source(s).");
            Console.Write("Target folder (Enter for default artifacts/raif-downloads): ");
            var inputFolder = Console.ReadLine()?.Trim();
            var outputFolder = string.IsNullOrWhiteSpace(inputFolder)
                ? Path.Combine("artifacts", "raif-downloads")
                : inputFolder;

            Directory.CreateDirectory(outputFolder);

            var successCount = 0;
            var failed = new List<string>();

            foreach (var source in candidates)
            {
                var outputFilePath = Path.Combine(outputFolder, BuildSafeFileName(source.Name));
                if (!outputFilePath.EndsWith(".raif", StringComparison.OrdinalIgnoreCase))
                    outputFilePath += ".raif";

                try
                {
                    var bytes = await ItemTransferClient.DownloadBlobSourceAsync(
                        destination,
                        source.Name,
                        outputFilePath,
                        CancellationToken.None);

                    successCount++;
                    Console.WriteLine($"   • Downloaded {source.Name} ({bytes} bytes) -> {outputFilePath}");
                }
                catch (Exception ex)
                {
                    failed.Add($"{source.Name}: {ex.Message}");
                }
            }

            if (failed.Count == 0)
            {
                VerificationHelper.PrintSuccess("Download Available RAIF Blobs To Disk", $"Downloaded {successCount} file(s) to '{outputFolder}'");
                return;
            }

            if (successCount == 0 && failed.All(f => f.Contains("200 JSON response (not binary)", StringComparison.OrdinalIgnoreCase)))
            {
                VerificationHelper.PrintFailure(
                    "Download Available RAIF Blobs To Disk",
                    "This environment's Item Transfer API returns blob metadata only and does not expose a binary RAIF download endpoint. " +
                    "Use the Content Transfer chunk stream for local archival, or consume the blob directly via Item Transfer.");
                return;
            }

            VerificationHelper.PrintFailure(
                "Download Available RAIF Blobs To Disk",
                $"Downloaded {successCount} file(s), failed {failed.Count}.\n" + string.Join("\n", failed.Take(5)));
        }

        private async Task ExportContentTransferChunksToDiskExample()
        {
            if (string.IsNullOrWhiteSpace(Config.TransferItemPath))
            {
                await VerificationHelper.LogAsync("TransferItemPath is empty; skipping chunk export example.", ConsoleColor.Yellow);
                return;
            }

            var dataTree = ContentMigrationWorkflow.CreateDataTreeFromConfig();
            PrintTransferSummary(dataTree);

            Console.Write("Target folder (Enter for default artifacts/transfer-chunks): ");
            var inputFolder = Console.ReadLine()?.Trim();
            var outputRoot = string.IsNullOrWhiteSpace(inputFolder)
                ? Path.Combine("artifacts", "transfer-chunks")
                : inputFolder;

            if (!VerificationHelper.PromptConfirmation("Start chunk export from source now?"))
                return;

            var source = await TransferEnvironmentContext.CreateSourceAsync();
            var transferId = Guid.NewGuid();

            var request = new CreateContentTransferRequest
            {
                TransferId = transferId,
                Configuration = new TransferConfiguration
                {
                    Database = Config.TransferDatabase,
                    DataTrees = new List<DataTree> { dataTree }
                }
            };

            await ContentTransferClient.CreateContentTransferAsync(source, request, CancellationToken.None);
            var status = await WaitForContentTransferCompletedForExportAsync(source, transferId, CancellationToken.None);

            var exportFolder = Path.Combine(outputRoot, transferId.ToString("D"));
            Directory.CreateDirectory(exportFolder);

            var manifest = new ChunkExportManifest
            {
                TransferId = transferId,
                SourceHost = Config.TransferSourceHost,
                Database = Config.TransferDatabase,
                ItemPath = dataTree.ItemPath,
                Scope = dataTree.Scope.ToString(),
                MergeStrategy = dataTree.MergeStrategy.ToString()
            };

            foreach (var chunkSet in status.ChunkSetsMetadata)
            {
                var setEntry = new ChunkSetExportEntry
                {
                    ChunkSetId = chunkSet.ChunkSetId,
                    ChunkCount = chunkSet.ChunkCount,
                    TotalItemCount = chunkSet.TotalItemCount
                };

                var chunkSetFolder = Path.Combine(exportFolder, $"chunkset-{chunkSet.ChunkSetId:D}");
                Directory.CreateDirectory(chunkSetFolder);

                for (var chunkId = 0; chunkId < chunkSet.ChunkCount; chunkId++)
                {
                    var chunk = await ContentTransferClient.GetChunkAsync(
                        source,
                        transferId,
                        chunkSet.ChunkSetId,
                        chunkId,
                        CancellationToken.None);

                    var chunkFileName = $"chunk-{chunkId:D5}.bin";
                    var chunkPath = Path.Combine(chunkSetFolder, chunkFileName);
                    await File.WriteAllBytesAsync(chunkPath, chunk.Data, CancellationToken.None);

                    setEntry.Chunks.Add(new ChunkExportEntry
                    {
                        ChunkId = chunkId,
                        FileName = chunkFileName,
                        Bytes = chunk.Data.LongLength,
                        IsMedia = chunk.IsMedia,
                        ItemsProcessed = chunk.ItemsProcessed,
                        ItemsSkipped = chunk.ItemsSkipped
                    });
                }

                manifest.ChunkSets.Add(setEntry);
            }

            var manifestPath = Path.Combine(exportFolder, "manifest.json");
            var manifestJson = JsonSerializer.Serialize(manifest, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(manifestPath, manifestJson, CancellationToken.None);

            await ContentTransferClient.DeleteContentTransferAsync(source, transferId, CancellationToken.None);

            var totalChunks = manifest.ChunkSets.Sum(s => s.Chunks.Count);
            var totalBytes = manifest.ChunkSets.Sum(s => s.Chunks.Sum(c => c.Bytes));
            VerificationHelper.PrintSuccess(
                "Export Content Transfer Chunks To Disk",
                $"Exported {totalChunks} chunk(s), {totalBytes} bytes to '{exportFolder}'. Manifest: {manifestPath}");
        }

        private static void PrintTransferSummary(DataTree dataTree)
        {
            Console.WriteLine("   Source host:      " + Config.TransferSourceHost);
            Console.WriteLine("   Destination host: " + Config.TransferDestinationHost);
            Console.WriteLine("   Database:         " + Config.TransferDatabase);
            Console.WriteLine("   Item path:        " + dataTree.ItemPath);
            Console.WriteLine("   Scope:            " + dataTree.Scope);
            Console.WriteLine("   Merge strategy:   " + dataTree.MergeStrategy);
            Console.WriteLine();
        }

        private static string BuildSafeFileName(string value)
        {
            var safe = value;
            foreach (var invalid in Path.GetInvalidFileNameChars())
            {
                safe = safe.Replace(invalid, '_');
            }

            return string.IsNullOrWhiteSpace(safe) ? "transfer" : safe;
        }

        private static async Task<ContentTransferCreationStatusResponse> WaitForContentTransferCompletedForExportAsync(
            TransferEnvironmentContext sourceContext,
            Guid transferId,
            CancellationToken cancellationToken)
        {
            const int maxAttempts = 120;
            var pollDelay = TimeSpan.FromSeconds(5);

            for (var attempt = 1; attempt <= maxAttempts; attempt++)
            {
                var status = await ContentTransferClient.GetContentTransferStatusAsync(sourceContext, transferId, cancellationToken);
                if (status == null)
                    throw new InvalidOperationException($"Content transfer '{transferId}' was not found.");

                if (status.State == ContentTransferState.Completed)
                    return status;

                if (status.State == ContentTransferState.Failed)
                    throw new InvalidOperationException($"Content transfer '{transferId}' failed in source environment.");

                await Task.Delay(pollDelay, cancellationToken);
            }

            throw new TimeoutException($"Content transfer '{transferId}' did not complete in time for chunk export.");
        }

        private sealed class ChunkExportManifest
        {
            public Guid TransferId { get; set; }

            public string SourceHost { get; set; } = string.Empty;

            public string Database { get; set; } = string.Empty;

            public string ItemPath { get; set; } = string.Empty;

            public string Scope { get; set; } = string.Empty;

            public string MergeStrategy { get; set; } = string.Empty;

            public List<ChunkSetExportEntry> ChunkSets { get; set; } = new();
        }

        private sealed class ChunkSetExportEntry
        {
            public Guid ChunkSetId { get; set; }

            public int ChunkCount { get; set; }

            public int TotalItemCount { get; set; }

            public List<ChunkExportEntry> Chunks { get; set; } = new();
        }

        private sealed class ChunkExportEntry
        {
            public int ChunkId { get; set; }

            public string FileName { get; set; } = string.Empty;

            public long Bytes { get; set; }

            public bool IsMedia { get; set; }

            public int? ItemsProcessed { get; set; }

            public int? ItemsSkipped { get; set; }
        }
    }
}