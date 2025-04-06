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
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using UserService.Data;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using UserService.Models;
using UserService.DTOs;
using Shared.Responses;
using UserService.Services.Interfaces;
using UserService.Controllers;

namespace UserService.Services
{
    public class NotificationService
    {
        private readonly UserDbContext _context;
        public NotificationService(
           UserDbContext context)
        {
            _context = context;
        }
        public async Task<ApiResponse<UserProfile>> GetUserInfo(string userId)
        {
            var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == int.Parse(userId));

            var res = new UserProfile
            {
                name = user.Username,
                phone = user.phone,
                company = user.company,
                department = user.department,
                position = user.position,
                email = user.Email,
                role = user.Role
            };

            return ApiResponse.Success(res);
        }

        public async Task<ApiResponse<string>> SetUserInfo(SetUserProfile temp, string userId)
        {
            // 根据用户ID查找用户
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == int.Parse(userId));

            // 检查 temp 中的字段是否为空，如果不为空则更新对应字段
            if (!string.IsNullOrWhiteSpace(temp.name))
            {
                user.Username = temp.name;
            }

            if (!string.IsNullOrWhiteSpace(temp.phone))
            {
                user.phone = temp.phone;
            }

            if (!string.IsNullOrWhiteSpace(temp.company))
            {
                user.company = temp.company;
            }

            if (!string.IsNullOrWhiteSpace(temp.department))
            {
                user.department = temp.department;
            }

            if (!string.IsNullOrWhiteSpace(temp.position))
            {
                user.position = temp.position;
            }

            if (!string.IsNullOrWhiteSpace(temp.email))
            {
                user.Email = temp.email;
            }

            await _context.SaveChangesAsync();

            // 返回成功响应，包含更新后的用户信息
            return ApiResponse.Success("ok");
        }

       

        
    }

}
