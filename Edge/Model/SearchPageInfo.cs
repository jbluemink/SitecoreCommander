namespace SitecoreCommander.Edge.Model
{
    internal class SearchPageInfo
    {
        public int total { get; set; }
        public PageInfo pageInfo { get; set; }
        public SearchResultItem[] results { get; set; }
    }
}
