using System.Text.Json.Serialization;

namespace UserService.Models
{
    public class WeChatTokenInfo
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; }

        [JsonPropertyName("openid")]
        public string OpenId { get; set; }

        [JsonPropertyName("scope")]
        public string Scope { get; set; }
    }
} 