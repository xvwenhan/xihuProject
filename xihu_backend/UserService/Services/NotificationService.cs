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

        public async Task SendSseNotifications(string userId, HttpResponse response)
        {
            // 设置响应头
            response.ContentType = "text/event-stream";
            response.Headers.Append("Cache-Control", "no-cache");
            response.Headers.Append("Connection", "keep-alive");

            while (true)
            {
                try
                {
                    Console.WriteLine($"用户 {userId} 正在接收通知...");

                    // 查询用户的通知
                    var notifications = GetNotificationsForUser(userId);
                    Console.WriteLine($"用户 {userId} 的通知: {JsonSerializer.Serialize(notifications)}");

                    if (notifications.Count > 0)
                    {
                        // 如果有通知，则发送通知
                        foreach (var notification in notifications)
                        {
                            await response.WriteAsync($"data: {JsonSerializer.Serialize(notification)}\n\n");
                            await response.Body.FlushAsync(); // 确保数据立即发送
                        }
                    }
                    else
                    {
                        // 如果没有通知，则发送心跳信息
                        await response.WriteAsync(": heartbeat\n\n");
                        await response.Body.FlushAsync();
                    }

                    // 每隔 1 分钟检查一次
                    await Task.Delay(TimeSpan.FromMinutes(1));
                }
                catch (Exception ex)
                {
                    // 记录异常并关闭连接
                    Console.Error.WriteLine($"SSE 错误: {ex.Message}");
                    break;
                }
            }
        }

        private List<object> GetNotificationsForUser(string userId)
        {
            // 获取当前本地时间
            var now = DateTime.Now;
            Console.WriteLine($"当前本地时间: {now}, 正在检索数据库...");

            // 查询用户订阅的会议
            var subscribedConferences = _context.Subscribes
                .Where(s => s.UserId == int.Parse(userId)) // 用户订阅的会议
                .Join(
                    _context.Conferences,
                    subscribe => subscribe.ConferenceId,
                    conference => conference.ConferenceId,
                    (subscribe, conference) => conference
                )
                .ToList();

            // 筛选出接下来 10 分钟内开始的会议
            var upcomingConferences = subscribedConferences
                .Where(c =>
                {
                    // 计算会议的完整时间（日期 + 开始时间）
                    var conferenceDateTime = (c.Date ?? DateTime.MinValue).Add(c.StartTime.HasValue ? c.StartTime.Value : TimeSpan.Zero);

                    // 比较时间是否在未来 10 分钟内
                    return conferenceDateTime > now && conferenceDateTime <= now.AddMinutes(10);
                })
                .Select(c => new
                {
                    conference_name = c.ConferenceName,
                    start_time = c.Date.GetValueOrDefault().Add(c.StartTime ?? TimeSpan.Zero).ToString("yyyy-MM-dd HH:mm:ss")
                })
                .ToList();

            Console.WriteLine($"找到 {upcomingConferences.Count} 条符合条件的会议。");

            // 返回符合条件的会议
            return upcomingConferences.Cast<object>().ToList();
        }
    }

}
     