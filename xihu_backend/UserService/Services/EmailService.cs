using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Mail;

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using UserService.Data;
using UserService.Models;
using UserService.DTOs;
using Shared.Responses;
using UserService.Services.Interfaces;

namespace UserService.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

         public async Task SendEmailAsync(string to, string subject, string body)
    {
        var smtpServer = _configuration["Email:SmtpServer"];
        var smtpPort = int.Parse(_configuration["Email:SmtpPort"]);
        var smtpUsername = _configuration["Email:Username"];
        var smtpPassword = _configuration["Email:Password"];
        var fromEmail = _configuration["Email:FromEmail"];

        var message = new MailMessage
        {
            From = new MailAddress(fromEmail),
            Subject = subject,
            Body = body,
            IsBodyHtml = false
        };
        message.To.Add(to);

        using (var client = new SmtpClient(smtpServer, smtpPort))
        {
            client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
            client.EnableSsl = true;

            try
            {
                await client.SendMailAsync(message);
            }
            catch (Exception ex)
            {
                throw new Exception("发送邮件失败", ex);
            }
        }
    }

    public async Task SendVerificationCodeAsync(string email, string code)
    {
        await SendEmailAsync(
            email,
            "验证码",
            $"您的验证码是: {code}，5分钟内有效。"
        );
    }
    }
}