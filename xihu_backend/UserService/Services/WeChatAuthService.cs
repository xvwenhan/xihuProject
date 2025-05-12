using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using UserService.Data;
using UserService.Models;
using UserService.Services.Interfaces;

namespace UserService.Services
{
    public class WeChatAuthService : IWeChatAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly UserDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly ILogger<WeChatAuthService> _logger;

        public WeChatAuthService(
            IConfiguration configuration,
            UserDbContext context,
            ILogger<WeChatAuthService> logger)
        {
            _configuration = configuration;
            _context = context;
            _logger = logger;
            _httpClient = new HttpClient();
        }

        public async Task<string> GetLoginUrl(string state)
        {
            var appId = _configuration["WeChat:AppId"];
            var redirectUrl = Uri.EscapeDataString(_configuration["WeChat:RedirectUrl"]);
            return $"https://open.weixin.qq.com/connect/oauth2/authorize?appid={appId}&redirect_uri={redirectUrl}&response_type=code&scope=snsapi_userinfo&state={state}#wechat_redirect";
        }

        public async Task<WeChatUserInfo> GetUserInfo(string code)
        {
            try
            {
                // 获取access_token
                var tokenUrl = $"https://api.weixin.qq.com/sns/oauth2/access_token?appid={_configuration["WeChat:AppId"]}&secret={_configuration["WeChat:AppSecret"]}&code={code}&grant_type=authorization_code";
                var tokenResponse = await _httpClient.GetStringAsync(tokenUrl);
                var tokenInfo = JsonSerializer.Deserialize<WeChatTokenInfo>(tokenResponse);

                if (tokenInfo == null || string.IsNullOrEmpty(tokenInfo.AccessToken))
                {
                    _logger.LogError("Failed to get access token from WeChat: {Response}", tokenResponse);
                    throw new Exception("Failed to get access token from WeChat");
                }

                // 获取用户信息
                var userInfoUrl = $"https://api.weixin.qq.com/sns/userinfo?access_token={tokenInfo.AccessToken}&openid={tokenInfo.OpenId}&lang=zh_CN";
                var userInfoResponse = await _httpClient.GetStringAsync(userInfoUrl);
                var userInfo = JsonSerializer.Deserialize<WeChatUserInfo>(userInfoResponse);

                if (userInfo == null)
                {
                    _logger.LogError("Failed to get user info from WeChat: {Response}", userInfoResponse);
                    throw new Exception("Failed to get user info from WeChat");
                }

                return userInfo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting WeChat user info");
                throw;
            }
        }

        public async Task<User> CreateOrUpdateUser(WeChatUserInfo weChatUser)
        {
            try
            {
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.WeChatOpenId == weChatUser.OpenId);

                if (existingUser == null)
                {
                    // 创建新用户
                    var newUser = new User
                    {
                        Username = weChatUser.NickName,
                        Email = $"{weChatUser.OpenId}@wechat.com",
                        PasswordHash = Guid.NewGuid().ToString(), // 随机密码，微信登录不需要
                        WeChatOpenId = weChatUser.OpenId,
                        LoginType = "WeChat",
                        Role = "Online", // 修改默认角色为 Online
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true
                    };

                    _context.Users.Add(newUser);
                    await _context.SaveChangesAsync();
                    return newUser;
                }
                else
                {
                    // 更新现有用户信息
                    existingUser.Username = weChatUser.NickName;
                    await _context.SaveChangesAsync();
                    return existingUser;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating or updating WeChat user");
                throw;
            }
        }
    }
} 