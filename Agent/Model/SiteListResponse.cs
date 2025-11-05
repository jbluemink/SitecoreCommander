namespace SitecoreCommander.Agent.Model
{
    public class SiteListResponse
    {
        public List<SiteInfo> Sites { get; set; } = new();
    }

    public class SiteInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string TargetHostname { get; set; } = string.Empty;
        public string RootPath { get; set; } = string.Empty;
    }
}
