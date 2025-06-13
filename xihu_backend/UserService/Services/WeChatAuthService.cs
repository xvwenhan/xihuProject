using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using UserService.Data;
using UserService.Models;
using UserService.Services.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using System.Security.Cryptography;
using System.Text;
using Shared.Responses;
using UserService.DTOs;

namespace UserService.Services
{
    public class WeChatAuthService : IWeChatAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly UserDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly ILogger<WeChatAuthService> _logger;
        private readonly IDistributedCache _cache;
        private readonly IAuthService _authService;

        public WeChatAuthService(
            IConfiguration configuration,
            UserDbContext context,
            ILogger<WeChatAuthService> logger,
            IAuthService authService,
            IDistributedCache cache)
        {
            _configuration = configuration;
            _context = context;
            _logger = logger;
            _httpClient = new HttpClient();
            _authService = authService;
            _cache = cache;
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
                var tokenUrl = $"https://api.weixin.qq.com/sns/oauth2/access_token?appid={_configuration["WeChat:AppId"]}&secret={_configuration["WeChat:AppSecret"]}&code={code}&grant_type=authorization_code";
                var tokenResponse = await _httpClient.GetStringAsync(tokenUrl);
                var tokenInfo = JsonSerializer.Deserialize<WeChatTokenInfo>(tokenResponse);

                if (tokenInfo == null || string.IsNullOrEmpty(tokenInfo.AccessToken))
                {
                    _logger.LogError("Failed to get access token from WeChat: {Response}", tokenResponse);
                    throw new Exception("Failed to get access token from WeChat");
                }

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
                    var newUser = new User
                    {
                        Username = weChatUser.NickName,
                        Email = $"{weChatUser.OpenId}@wechat.com",
                        PasswordHash = Guid.NewGuid().ToString(),
                        WeChatOpenId = weChatUser.OpenId,
                        LoginType = "WeChat",
                        Role = "Online",
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true
                    };

                    _context.Users.Add(newUser);
                    await _context.SaveChangesAsync();
                    return newUser;
                }
                else
                {
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

        /// <summary>
        /// 验证微信服务器签名
        /// </summary>
        public async Task<ApiResponse<object>> VerifyWeChatSignatureAsync(string signature, string timestamp, string nonce, string echostr)
        {
            try
            {
                var token = _configuration["WeChat:Token"];
                var arr = new[] { token, timestamp, nonce }.OrderBy(x => x).ToArray();
                var str = string.Join("", arr);

                using var sha1 = SHA1.Create();
                var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(str));
                var signatureStr = BitConverter.ToString(hash).Replace("-", "").ToLower();

                if (signatureStr == signature)
                {
                    _logger.LogInformation("微信验证成功");
                    return ApiResponse.Success<object>(echostr);
                }

                _logger.LogWarning($"微信验证失败，本地签名：{signatureStr}，微信签名：{signature}");
                return ApiResponse.Fail<object>("微信签名验证失败");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "微信验证过程发生异常");
                return ApiResponse.Fail<object>($"服务器异常：{ex.Message}");
            }
        }

        /// <summary>
        /// 获取微信登录URL
        /// </summary>
        public async Task<ApiResponse<object>> GetLoginUrlAsync()
        {
            try
            {
                var state = Guid.NewGuid().ToString("N");
                var appId = _configuration["WeChat:AppId"];
                var redirectUrl = Uri.EscapeDataString(_configuration["WeChat:RedirectUrl"]);
                var url = $"https://open.weixin.qq.com/connect/oauth2/authorize?appid={appId}&redirect_uri={redirectUrl}&response_type=code&scope=snsapi_userinfo&state={state}#wechat_redirect";

                return ApiResponse.Success<object>(new { url, state });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取微信登录URL失败");
                return ApiResponse.Fail<object>($"获取微信登录URL失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 处理微信回调
        /// </summary>
        public async Task<ApiResponse<object>> HandleCallbackAsync(string code, string state)
        {
            try
            {
                var weChatUser = await GetUserInfo(code);
                var user = await CreateOrUpdateUser(weChatUser);
                var token = _authService.GenerateJwtToken(user.Id.ToString(), user.Role);

                var loginInfo = new
                {
                    token,
                    user = new
                    {
                        user.Id,
                        user.Username,
                        user.Email,
                        user.Role,
                        user.LoginType
                    }
                };

                await _cache.SetStringAsync(
                    $"wechat_login_{state}",
                    JsonSerializer.Serialize(loginInfo),
                    new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                    }
                );

                return ApiResponse.Success<object>(loginInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理微信回调失败");
                return ApiResponse.Fail<object>($"处理微信回调失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 检查登录状态
        /// </summary>
        public async Task<ApiResponse<object>> CheckLoginStateAsync(string state)
        {
            try
            {
                var loginInfoJson = await _cache.GetStringAsync($"wechat_login_{state}");

                if (string.IsNullOrEmpty(loginInfoJson))
                {
                    return ApiResponse.Success<object>(new { isLoggedIn = false });
                }

                var loginInfo = JsonSerializer.Deserialize<object>(loginInfoJson);
                return ApiResponse.Success<object>(new { isLoggedIn = true, data = loginInfo });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "检查登录状态失败");
                return ApiResponse.Fail<object>($"检查登录状态失败：{ex.Message}");
            }
        }

        /// <summary>
        /// 绑定微信账号
        /// </summary>
        public async Task<ApiResponse<object>> BindWeChatAsync(int userId, string code)
        {
            try
            {
                var weChatUser = await GetUserInfo(code);
                var existingUser = await _authService.GetUserByWeChatOpenId(weChatUser.OpenId);
                if (existingUser != null)
                {
                    return ApiResponse.Fail<object>("该微信账号已被其他用户绑定");
                }

                var user = await _authService.BindWeChat(userId, weChatUser.OpenId);

                return ApiResponse.Success<object>(new
                {
                    message = "微信绑定成功",
                    user = new
                    {
                        user.Id,
                        user.Username,
                        user.Email,
                        user.Role,
                        user.LoginType
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "绑定微信账号失败");
                return ApiResponse.Fail<object>($"绑定微信账号失败：{ex.Message}");
            }
        }
    }
}
