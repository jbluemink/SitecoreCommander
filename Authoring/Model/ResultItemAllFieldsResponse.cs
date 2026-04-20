namespace SitecoreCommander.Authoring.Model
{
    public class ResultItemAllFieldsResponse
    {
        public ResultItemWithAllFields? item { get; set; }
    }

    public class ItemWithAllFieldsPageInfo
    {
        public bool hasNextPage { get; set; }
        public string? endCursor { get; set; }
    }

    public class ItemFieldsConnection
    {
        public ItemWithAllFieldsPageInfo pageInfo { get; set; } = null!;
        public ItemFieldInfo[] nodes { get; set; } = [];
    }

    public class ItemWithAllFieldsAndConnection
    {
        public string itemId { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
        public string path { get; set; } = string.Empty;
        public Language language { get; set; } = null!;
        public int version { get; set; }
        public ResultValue? created { get; set; }
        public ResultValue? updated { get; set; }
        public ResultValue? security { get; set; }
        public ResultTemplateInfo? template { get; set; }
        public ItemFieldsConnection fields { get; set; } = null!;
        public Access access { get; set; } = null!;
    }

    public class ResultItemAllFieldsWithConnectionResponse
    {
        public ItemWithAllFieldsAndConnection? item { get; set; }
    }
}
