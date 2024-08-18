using System.Text.Json.Serialization;

namespace SitecoreCommander.RESTful.Model
{
    public class Authentication
    {
        [JsonPropertyName("domain")]
        public string Domain { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("password")]
        public string Password { get; set; }
    }
}
