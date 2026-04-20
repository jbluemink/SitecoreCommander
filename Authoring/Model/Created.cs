namespace SitecoreCommander.Authoring.Model
{
    public class Created
    {
        public string itemId { get; set; } = string.Empty;

        public string itemIdEnclosedInBraces
        {
            get { return Guid.Parse(itemId).ToString("B").ToUpper(); }
        }
    }
}
