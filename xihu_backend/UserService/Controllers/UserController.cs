using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserService.DTOs;
using UserService.Models;
using Shared.Responses;
using UserService.Data;
using BCrypt.Net;
//using Microsoft.IdentityModel.Tokens;
//using System.IdentityModel.Tokens.Jwt;
//using System.Security.Claims;
//using System.Text;
using UserService;
using UserService.Services;
using UserService.Services.Interfaces;

namespace UserService.Controllers
{
    [ApiController]
    [Route("api/user")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly NotificationService _notificationService;

        public AuthController(IAuthService authService, NotificationService notificationService)
        {
            _authService = authService;
            _notificationService = notificationService;
        }

        /// <summary>
        /// 用户注册接口
        /// </summary>
        [HttpPost("public/register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            var response = await _authService.NewRegisterAsync(request);
            return response.Success ? Ok(response) : BadRequest(response);
        }

        /// <summary>
        /// 用户登录接口
        /// </summary>
        [HttpPost("public/login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var response = await _authService.LoginAsync(request);
            return response.Success ? Ok(response) : Unauthorized(response);
        }

        /// <summary>
        /// 发送邮箱验证码
        /// </summary>
       [HttpPost("public/send-verification-code")]
       public async Task<IActionResult> SendVerificationCode(SendVerificationCodeRequest request)
       {
           var response = await _authService.SendVerificationCodeAsync(request);
           return response.Success ? Ok(response) : BadRequest(response);
       }

       /// <summary>
       /// 验证邮箱验证码
       /// </summary>
       [HttpPost("public/verify-code")]
       public async Task<IActionResult> VerifyCode(VerifyCodeRequest request)
       {
           var response = await _authService.VerifyCodeAsync(request);
           return response.Success ? Ok(response) : BadRequest(response);
       }


        /// <summary>
        /// 获取用户信息
        /// </summary>
        [HttpGet("private/getUserInfo")]
        public async Task<IActionResult> GetUserInfo()
        {
            var userId = Request.Headers["X-User-Id"].FirstOrDefault();
            var role = Request.Headers["X-User-Role"].FirstOrDefault();
            var response = await _notificationService.GetUserInfo(userId);
            return response.Success ? Ok(response) : BadRequest(response);
        }
        /// <summary>
        /// 设置用户信息
        /// </summary>
        [HttpPost("private/setUserInfo")]
        public async Task<IActionResult> SetUserInfo([FromBody] SetUserProfile request)
        {
            var userId = Request.Headers["X-User-Id"].FirstOrDefault();
            var response = await _notificationService.SetUserInfo(request, userId);
            return response.Success ? Ok(response) : BadRequest(response);
        }
    }
}