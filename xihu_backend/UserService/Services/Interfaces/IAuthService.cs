using System.Threading.Tasks;
using UserService.DTOs;
using Shared.Responses;
using UserService.Services;
using UserService.Models;

namespace UserService.Services.Interfaces
{
    public interface IAuthService
    {

        /// <summary>
        /// 用户注册
        /// </summary>
        Task<ApiResponse<object>> RegisterAsync(InternalRegisterRequest request);
        Task<ApiResponse<object>> NewRegisterAsync(RegisterRequest request);

        /// <summary>
        /// 用户登录
        /// </summary>
        Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request);

        /// <summary>
        /// 用户认证
        /// </summary>
        Task<(bool success, string message)> ValidateUserAsync(string username, string password);
        Task<ApiResponse<bool>> SendVerificationCodeAsync(SendVerificationCodeRequest request);
        Task<ApiResponse<bool>> VerifyCodeAsync(VerifyCodeRequest request);

        public string GenerateJwtToken(string userId, string role);

        Task<User> Register(string username, string email, string password);
        Task<string> Login(string email, string password);
        Task<User> GetUserById(int id);
        Task<bool> ChangePassword(int userId, string oldPassword, string newPassword);
        Task<bool> ResetPassword(string email, string newPassword);
        Task<User> GetUserByEmail(string email);
        Task<User> GetUserByWeChatOpenId(string openId);
        Task<User> BindWeChat(int userId, string openId);
    }

}