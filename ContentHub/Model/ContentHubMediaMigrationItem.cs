namespace SitecoreCommander.ContentHub.Model
{
    internal sealed class ContentHubMediaMigrationItem
    {
        public string FileName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string AltText { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? SourceUrl { get; set; }

        // Populated after asset creation for downstream usage
        public string? ContentHubIdentifier { get; set; }
        public string? ContentHubPublicHash { get; set; }
        public string? ContentHubVersionHash { get; set; }
    }
}
