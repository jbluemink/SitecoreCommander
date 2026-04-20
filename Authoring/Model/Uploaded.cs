namespace SitecoreCommander.Authoring.Model
{
    public class Uploaded
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string ItemPath { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;

        public string ItemIdEnclosedInBraces
        {
            get { return Guid.Parse(Id).ToString("B").ToUpper(); }
        }
    }
}
