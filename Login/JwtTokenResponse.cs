
namespace SitecoreCommander.Login
{
    public class JwtTokenResponse
    {
        public string access_token { get; set; } = string.Empty;
        public string scope { get; set; } = string.Empty;
        public int expires_in { get; set; }
        public string token_type { get; set; } = string.Empty;

        public DateTime ExpiresAt => DateTime.UtcNow.AddSeconds(expires_in);
    }
}
