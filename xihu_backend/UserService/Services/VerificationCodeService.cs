using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Caching.Distributed;

using UserService.Data;
using UserService.Models;
using UserService.DTOs;
using Shared.Responses;
using UserService.Services.Interfaces;

namespace UserService.Services
{
    public class VerificationCodeService : IVerificationCodeService
    {
        private readonly IDistributedCache _cache;
        private const int CODE_LENGTH = 6;
        private const int CODE_EXPIRATION_MINUTES = 5;

        public VerificationCodeService(IDistributedCache cache)
        {
            _cache = cache;
        }

        public string GenerateCode()
        {
            // 生成6位数字验证码
            return RandomNumberGenerator
                .GetBytes(4)
                .Select(b => b % 10)
                .Aggregate("", (s, i) => s + i.ToString())
                .PadRight(CODE_LENGTH, '0')
                .Substring(0, CODE_LENGTH);
        }

        public async Task SaveCodeAsync(string email, string code)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CODE_EXPIRATION_MINUTES)
            };

            await _cache.SetStringAsync(
                $"verification_code:{email}",
                code,
                options
            );
        }

        public async Task<bool> ValidateCodeAsync(string email, string code)
        {
            var savedCode = await _cache.GetStringAsync($"verification_code:{email}");

            if (string.IsNullOrEmpty(savedCode))
                return false;

            return savedCode.Equals(code, StringComparison.OrdinalIgnoreCase);
        }

        public async Task RemoveCodeAsync(string email)
        {
            await _cache.RemoveAsync($"verification_code:{email}");
        }
    }
}