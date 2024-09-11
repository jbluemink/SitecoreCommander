using System.Globalization;

namespace SitecoreCommander.Edge.Model
{ 
    internal class ResultGetItem
    {
        public string id { get; set; }
        public string name { get; set; }
        public string path { get; set; }
        public ResultValue created { get; set; }
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
