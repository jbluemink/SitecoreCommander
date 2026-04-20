namespace SitecoreCommander.Authoring.Model
{
    public class ResultSearchWithLayouts
    {
        public InnerItemSearchWithLayouts[] results { get; set; } = Array.Empty<InnerItemSearchWithLayouts>();
        public int totalCount { get; set; }
    }
}
