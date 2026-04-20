namespace SitecoreCommander.Authoring.Model
{
    public class ResultItemChildrenWithSecurityChildren
    {
        public PageInfo pageInfo { get; set; } = null!;
        public ResultItemWithSecurity[] nodes { get; set; } = Array.Empty<ResultItemWithSecurity>();
    }
}
