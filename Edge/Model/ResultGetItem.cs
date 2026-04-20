using System.Globalization;

namespace SitecoreCommander.Edge.Model
{ 
    internal class ResultGetItem
    {
        public string id { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
        public string path { get; set; } = string.Empty;
        public ResultValue created { get; set; } = null!;
        public string itemIdEnclosedInBraces
        {
            get { return Guid.Parse(id).ToString("B").ToUpper(); }
        }

        public DateTime? CreatedDateTime
        {
            get
            {
                if (string.IsNullOrEmpty(created.value))
                {
                    return null;
                }

                // Definieer het formaat van de datumtijd string
                string format = "yyyyMMddTHHmmssZ";

                // Probeer de string te converteren naar DateTime
                if (DateTime.TryParseExact(created.value, format, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out DateTime result))
                {
                    return result;
                }

                // Als de string niet succesvol is geconverteerd, geef null terug
                return null;
            }
        }
    }
}
