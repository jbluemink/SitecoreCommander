namespace SitecoreCommander.Authoring.Model
{
    internal class ResultSearchWithField
    {
        public InnerItemSearchWithField[] results { get; set; }
        public int totalCount { get; set; }
    }
}
