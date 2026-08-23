namespace SitecoreCommander.ItemTransfer.Model
{
    public class TransfersPagedResponse
    {
        public int Page { get; set; }

        public int PageSize { get; set; }

        public int TotalCount { get; set; }

        public List<TransferStatusResult> Transfers { get; set; } = new();
    }

    public class TransferStatusResult
    {
        public string Id { get; set; } = string.Empty;

        public string SourceName { get; set; } = string.Empty;

        public string DatabaseName { get; set; } = string.Empty;

        public DateTimeOffset? ConsumedDate { get; set; }

        public ItemTransferState TransferState { get; set; }

        public TransferMergeStrategy Strategy { get; set; }

        public string Description { get; set; } = string.Empty;
    }

    public class TransferDetailsResult : TransferStatusResult
    {
        public int TotalItemsCount { get; set; }

        public int TransferredItemsCount { get; set; }

        public List<string>? ValidationErrors { get; set; }

        public int SourcesCount { get; set; }
    }

    public enum ItemTransferState
    {
        Unknown,
        InProgress,
        Finished,
        Failed,
        Queued,
        Discarded
    }

    public enum TransferMergeStrategy
    {
        OverrideExistingItem,
        KeepExistingItem,
        OverrideExistingTree
    }

    public class StartItemsTransferResponse
    {
        public string Location { get; set; } = string.Empty;

        public string SourceName { get; set; } = string.Empty;
    }

    public class RetryResult
    {
        public string DatabaseName { get; set; } = string.Empty;

        public string SourceName { get; set; } = string.Empty;
    }

    public class ListItemsResult
    {
        public int Page { get; set; }

        public int PageSize { get; set; }

        public int TotalCount { get; set; }

        public List<ItemData> Items { get; set; } = new();
    }

    public class ItemData
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public Guid ParentId { get; set; }

        public Guid TemplateId { get; set; }

        public Guid MasterId { get; set; }

        public bool IsTransferred { get; set; }

        public long TimeStamp { get; set; }

        public DateTimeOffset? TimeStampDate { get; set; }

        public string SourceName { get; set; } = string.Empty;
    }

    public class ItemDetailsResult : ItemData
    {
        public List<FieldData> Fields { get; set; } = new();
    }

    public class FieldData
    {
        public Guid Id { get; set; }

        public string Value { get; set; } = string.Empty;

        public string? Language { get; set; }

        public int? Version { get; set; }
    }

    public class BlobSourcesResult
    {
        public int Page { get; set; }

        public int PageSize { get; set; }

        public int TotalCount { get; set; }

        public List<BlobSourceInfo> Sources { get; set; } = new();
    }

    public class BlobSourceInfo
    {
        public string Name { get; set; } = string.Empty;

        public BlobState BlobState { get; set; }
    }

    public class BlobDetailsResult
    {
        public BlobState BlobState { get; set; }

        public string? Error { get; set; }

        public string SourceName { get; set; } = string.Empty;
    }

    public enum BlobState
    {
        Unknown,
        Uploading,
        Uploaded,
        Initializing,
        Error,
        Consumed,
        Transferred,
        TransferredWithErrors,
        Queued,
        Discarded
    }

    public class FileSourcesResult
    {
        public int Page { get; set; }

        public int PageSize { get; set; }

        public int TotalCount { get; set; }

        public List<FileSourceInfo> Sources { get; set; } = new();
    }

    public class FileSourceInfo
    {
        public string FileName { get; set; } = string.Empty;

        public string DatabaseName { get; set; } = string.Empty;
    }

    public class HistoryResult
    {
        public int Page { get; set; }

        public int PageSize { get; set; }

        public int TotalCount { get; set; }

        public List<TransfersHistory> Sources { get; set; } = new();
    }

    public class TransfersHistory
    {
        public string Name { get; set; } = string.Empty;

        public string SourceName { get; set; } = string.Empty;

        public DateTimeOffset? ConsumeDate { get; set; }

        public TransferMergeStrategy Strategy { get; set; }

        public List<HistoryEvent> Events { get; set; } = new();
    }

    public class HistoryEvent
    {
        public ItemTransferState Name { get; set; }

        public DateTimeOffset? Date { get; set; }
    }

    public class ErrorResponse
    {
        public string Error { get; set; } = string.Empty;
    }
}