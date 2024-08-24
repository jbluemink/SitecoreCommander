namespace SitecoreCommander.Authoring.Model
{
    internal class ResultSearchWithSecurity
    {
        public InnerItemSearchWithSecurity[] results { get; set; }
        public int totalCount { get; set; }
    }
}
