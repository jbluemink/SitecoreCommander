namespace SitecoreCommander.Authoring.Model
{
    internal class Uploaded
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ItemPath { get; set; }
        public string Message { get; set; }

        public string ItemIdEnclosedInBraces
        {
            get { return Guid.Parse(Id).ToString("B").ToUpper(); }
        }
    }
}
