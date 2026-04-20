namespace SitecoreCommander.ContentHub.Model
{
    internal sealed class ContentHubImportResult
    {
        public bool Success { get; set; }
        public long? AssetId { get; set; }
        public bool Created { get; set; }
        public bool Updated { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
