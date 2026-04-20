namespace SitecoreCommander.Authoring.Model
{
    public class PublishItemResponse
    {
        public PublishItemData publishItem { get; set; } = null!;
    }

    public class PublishItemData
    {
        public string operationId { get; set; } = string.Empty;
    }
}
