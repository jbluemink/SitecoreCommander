namespace SitecoreCommander.ContentTransfer.Model
{
    public class CreateContentTransferRequest
    {
        public TransferConfiguration Configuration { get; set; } = new();

        public Guid TransferId { get; set; }
    }

    public class TransferConfiguration
    {
        public List<DataTree> DataTrees { get; set; } = new();

        public string Database { get; set; } = "master";
    }

    public class DataTree
    {
        public string ItemPath { get; set; } = string.Empty;

        public DataTreeScope Scope { get; set; } = DataTreeScope.SingleItem;

        public TransferMergeStrategy MergeStrategy { get; set; } = TransferMergeStrategy.OverrideExistingItem;
    }

    public enum DataTreeScope
    {
        SingleItem,
        ItemAndDescendants
    }

    public enum TransferMergeStrategy
    {
        OverrideExistingItem,
        KeepExistingItem,
        OverrideExistingTree
    }

    public class ContentTransferCreationStatusResponse
    {
        public ContentTransferState State { get; set; }

        public List<ChunkSetMetadata> ChunkSetsMetadata { get; set; } = new();
    }

    public enum ContentTransferState
    {
        Running,
        Completed,
        Failed,
        NotFound
    }

    public class ChunkSetMetadata
    {
        public Guid ChunkSetId { get; set; }

        public int ChunkCount { get; set; }

        public int TotalItemCount { get; set; }
    }

    public class ChunkSetCompleteResponse
    {
        public string ContentTransferFileName { get; set; } = string.Empty;
    }

    public class ContentTransferChunk
    {
        public byte[] Data { get; set; } = Array.Empty<byte>();

        public bool IsMedia { get; set; }

        public int? ItemsProcessed { get; set; }

        public int? ItemsSkipped { get; set; }
    }

    public class ErrorResponse
    {
        public string Error { get; set; } = string.Empty;
    }
}