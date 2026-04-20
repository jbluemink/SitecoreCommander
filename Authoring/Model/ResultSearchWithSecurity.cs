namespace SitecoreCommander.Authoring.Model
{
    public class ResultSearchWithSecurity
    {
        public InnerItemSearchWithSecurity[] results { get; set; } = Array.Empty<InnerItemSearchWithSecurity>();
        public int totalCount { get; set; }
    }
}
