namespace SitecoreCommander.Authoring.Model
{
    internal class Created
    {
        public string itemId { get; set; }

        public string itemIdEnclosedInBraces
        {
            get { return Guid.Parse(itemId).ToString("B").ToUpper(); }
        }
    }
}
