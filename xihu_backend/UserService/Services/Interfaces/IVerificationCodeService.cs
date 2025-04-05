using System.Threading.Tasks;
using UserService.DTOs;
using Shared.Responses;
using UserService.Services;

namespace UserService.Services.Interfaces
{
    public interface IVerificationCodeService
    {
        string GenerateCode();
        Task SaveCodeAsync(string email, string code);
        Task<bool> ValidateCodeAsync(string email, string code);
        Task RemoveCodeAsync(string email);
    }
}
