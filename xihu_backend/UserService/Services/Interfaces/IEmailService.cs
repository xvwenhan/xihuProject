
using System.Threading.Tasks;
using UserService.DTOs;
using Shared.Responses;
using UserService.Services;


namespace UserService.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendVerificationCodeAsync(string email, string code);
        Task SendEmailAsync(string to, string subject, string body);
    }
}
