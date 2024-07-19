namespace SitecoreCommander.Edge.Model
{ 
    internal class ResultGetItem
    {
        public string id { get; set; }
        public string name { get; set; }
        public string path { get; set; }
        public string itemIdEnclosedInBraces
        {
            get { return Guid.Parse(id).ToString("B").ToUpper(); }
        }
    }
}
