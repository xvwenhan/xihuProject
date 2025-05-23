using System.Text.Json.Serialization;

namespace UserService.Models
{
    public class WeChatUserInfo
    {
        [JsonPropertyName("openid")]
        public string OpenId { get; set; }

        [JsonPropertyName("nickname")]
        public string NickName { get; set; }

        [JsonPropertyName("sex")]
        public int Sex { get; set; }

        [JsonPropertyName("language")]
        public string Language { get; set; }

        [JsonPropertyName("city")]
        public string City { get; set; }

        [JsonPropertyName("province")]
        public string Province { get; set; }

        [JsonPropertyName("country")]
        public string Country { get; set; }

        [JsonPropertyName("headimgurl")]
        public string HeadImgUrl { get; set; }

        [JsonPropertyName("privilege")]
        public List<string> Privilege { get; set; }
    }
} 