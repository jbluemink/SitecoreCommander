namespace SitecoreCommander.Authoring.Model
{
    public class ResultSearchWithField
    {
        public InnerItemSearchWithField[] results { get; set; } = Array.Empty<InnerItemSearchWithField>();
        public int totalCount { get; set; }
    }
}
