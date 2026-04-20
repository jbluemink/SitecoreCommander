using System.Text.Json.Serialization;

namespace SitecoreCommander.RESTful.Model
{
    public class Authentication
    {
        [JsonPropertyName("domain")]
        public string Domain { get; set; } = string.Empty;

        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;

        [JsonPropertyName("password")]
        public string Password { get; set; } = string.Empty;
    }
}
