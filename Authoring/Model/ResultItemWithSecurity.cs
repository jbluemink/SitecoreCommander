using System.Globalization;

namespace SitecoreCommander.Authoring.Model
{
    internal class ResultItemWithSecurity
    {
        public int version { get; set; }
        public Language language { get; set; }
        public string itemId { get; set; }
        public string name { get; set; }
        public string path { get; set; }
        public ResultValue security { get; set; }
        public ResultValue created { get; set; }
        public Access access { get; set; }

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
