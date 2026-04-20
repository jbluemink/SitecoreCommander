using System.Globalization;

namespace SitecoreCommander.Authoring.Model
{
    public class ResultItemWithSecurity
    {
        public int version { get; set; }
        public Language language { get; set; } = null!;
        public string itemId { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
        public string path { get; set; } = string.Empty;
        public ResultValue security { get; set; } = null!;
        public ResultValue created { get; set; } = null!;
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
}
