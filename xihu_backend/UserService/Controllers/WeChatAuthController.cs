using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.Models;
using UserService.Services.Interfaces;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Caching.Distributed;

namespace UserService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WeChatAuthController : ControllerBase
    {
        private readonly IWeChatAuthService _weChatAuthService;
        private readonly IAuthService _authService;
        private readonly ILogger<WeChatAuthController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IDistributedCache _cache;

        public WeChatAuthController(
            IWeChatAuthService weChatAuthService,
            IAuthService authService,
            ILogger<WeChatAuthController> logger,
            IConfiguration configuration,
            IDistributedCache cache)
        {
            _weChatAuthService = weChatAuthService;
            _authService = authService;
            _logger = logger;
            _configuration = configuration;
            _cache = cache;
        }

        [HttpGet]
        public IActionResult WxCheck([FromQuery] string signature, [FromQuery] string timestamp, [FromQuery] string nonce, [FromQuery] string echostr)
        {
            try
            {
                // 获取配置的Token
                var token = _configuration["WeChat:Token"];
                
                // 1. 将token、timestamp、nonce三个参数进行字典序排序
                var arr = new[] { token, timestamp, nonce }.OrderBy(x => x).ToArray();
                
                // 2. 将三个参数字符串拼接成一个字符串
                var str = string.Join("", arr);
                
                // 3. 进行sha1加密
                var sha1 = SHA1.Create();
                var hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(str));
                var signatureStr = BitConverter.ToString(hash).Replace("-", "").ToLower();
                
                // 4. 开发者获得加密后的字符串可与signature对比，标识该请求来源于微信
                if (signatureStr == signature)
                {
                    _logger.LogInformation("微信验证成功");
                    return Content(echostr);
                }
                
                _logger.LogWarning($"微信验证失败，本地签名：{signatureStr}，微信签名：{signature}");
                return BadRequest("验证失败");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "微信验证过程发生错误");
                return BadRequest("验证过程发生错误");
            }
        }

        [HttpGet("login-url")]
        public async Task<IActionResult> GetLoginUrl()
        {
            try
            {
                var state = Guid.NewGuid().ToString("N"); // 生成随机state
                var url = await _weChatAuthService.GetLoginUrl(state);
                return Ok(new { url });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting WeChat login URL");
                return BadRequest(new { message = "获取微信登录URL失败" });
            }
        }

        [HttpGet("callback")]
        public async Task<IActionResult> Callback([FromQuery] string code, [FromQuery] string state)
        {
            try
            {
                var weChatUser = await _weChatAuthService.GetUserInfo(code);
                var user = await _weChatAuthService.CreateOrUpdateUser(weChatUser);
                
                // 生成JWT token
                var token = _authService.GenerateJwtToken(user.Id.ToString(), user.Role);

                // 将登录信息存储到缓存中
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
                    System.Text.Json.JsonSerializer.Serialize(loginInfo),
                    new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
                    }
                );
                
                return Ok(loginInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in WeChat callback");
                return BadRequest(new { message = "微信登录失败", error = ex.Message });
            }
        }

        [HttpGet("check-state")]
        public async Task<IActionResult> CheckState([FromQuery] string state)
        {
            try
            {
                var loginInfoJson = await _cache.GetStringAsync($"wechat_login_{state}");
                if (string.IsNullOrEmpty(loginInfoJson))
                {
                    return Ok(new { isLoggedIn = false });
                }

                var loginInfo = System.Text.Json.JsonSerializer.Deserialize<object>(loginInfoJson);
                return Ok(new { isLoggedIn = true, data = loginInfo });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking login state");
                return BadRequest(new { message = "检查登录状态失败", error = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("bind")]
        public async Task<IActionResult> BindWeChat([FromQuery] string code)
        {
            try
            {
                var userId = int.Parse(User.FindFirst("id")?.Value);
                var weChatUser = await _weChatAuthService.GetUserInfo(code);
                
                // 检查是否已被其他用户绑定
                var existingUser = await _authService.GetUserByWeChatOpenId(weChatUser.OpenId);
                if (existingUser != null)
                {
                    return BadRequest(new { message = "该微信账号已被其他用户绑定" });
                }

                // 绑定微信
                var user = await _authService.BindWeChat(userId, weChatUser.OpenId);
                
                return Ok(new
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
                _logger.LogError(ex, "Error binding WeChat account");
                return BadRequest(new { message = "微信绑定失败", error = ex.Message });
            }
        }
    }
} 