 using UserService.Models;
using Shared.Responses;

namespace UserService.Services.Interfaces
{
    public interface IWeChatAuthService
    {
        Task<string> GetLoginUrl(string state);
        Task<WeChatUserInfo> GetUserInfo(string code);
        Task<User> CreateOrUpdateUser(WeChatUserInfo weChatUser);

        Task<ApiResponse<object>> VerifyWeChatSignatureAsync(string signature, string timestamp, string nonce, string echostr);
        Task<ApiResponse<object>> GetLoginUrlAsync();
        Task<ApiResponse<object>> HandleCallbackAsync(string code, string state);
        Task<ApiResponse<object>> CheckLoginStateAsync(string state);
        Task<ApiResponse<object>> BindWeChatAsync(int userId, string code);
    }
}  