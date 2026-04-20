using System.Globalization;

namespace SitecoreCommander.Authoring.Model
{
    public class ResultItemWithAllFields
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
        public Dictionary<string, ResultValue> fields { get; set; } = [];
        public Access access { get; set; } = null!;

        public string itemIdEnclosedInBraces
        {
            get { return Guid.Parse(itemId).ToString("B").ToUpper(); }
        }

        public DateTime? CreatedDateTime
        {
            get
            {
                if (string.IsNullOrEmpty(created?.value))
                {
                    return null;
                }

                string format = "yyyyMMddTHHmmssZ";

                if (DateTime.TryParseExact(created.value, format, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out DateTime result))
                {
                    return result;
                }

                return null;
            }
        }
    }

    public class ItemFieldInfo
    {
        public string name { get; set; } = string.Empty;
        public string displayName { get; set; } = string.Empty;
        public string? value { get; set; }
    }
}
