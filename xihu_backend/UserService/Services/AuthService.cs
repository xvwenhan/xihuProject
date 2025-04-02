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
using UserService.Models;
using UserService.DTOs;
using Shared.Responses;
using UserService.Services.Interfaces;
using UserService.Controllers;

namespace UserService.Services
{

public class AuthService : IAuthService
{
    private readonly UserDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly IEmailService _emailService;
    private readonly IDistributedCache _cache;
    private readonly ILogger<AuthService> _logger;

    public AuthService(UserDbContext context, IConfiguration configuration, IEmailService emailService,
        IDistributedCache cache,
        ILogger<AuthService> logger)
    {
        _context = context;
        _configuration = configuration;
        _emailService = emailService;
        _cache = cache;
        _logger = logger;
    }

    public async Task<ApiResponse<object>> NewRegisterAsync(RegisterRequest request)
{
    try
    {
        // 检查邮箱是否已注册
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
        {
            return ApiResponse.Fail<object>("邮箱已被注册");
        }

        // 验证验证码
        string cacheKey = $"verification_code:{request.Email}";
        string storedCode = await _cache.GetStringAsync(cacheKey);

        if (string.IsNullOrEmpty(storedCode))
        {
            return ApiResponse.Fail<object>("验证码已过期", "CODE_EXPIRED");
        }

        if (storedCode != request.Code)  // 修改这里使用 Code
        {
            return ApiResponse.Fail<object>("验证码不正确", "INVALID_CODE");
        }

        // 验证成功后删除验证码
        await _cache.RemoveAsync(cacheKey);

        // 创建新用户
        var newUser = new User
        {
            Username = "user",
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            Role = "Online"
        };

        _context.Users.Add(newUser);
        await _context.SaveChangesAsync();

        return ApiResponse.Success<object>(null, "注册成功");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "注册失败");
        return ApiResponse.Fail<object>($"注册失败: {ex.Message}");
    }
}


    public async Task<ApiResponse<object>> RegisterAsync(InternalRegisterRequest request)
    {
        try
        {
           if (await _context.Users.AnyAsync(u => u.Email == request.Email))
        {
            return ApiResponse.Fail<object>("邮箱已被注册");
        }

             var newUser = new User
        {
            Username = "user",
            Email = request.Email, // 添加邮箱
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            Role = "Online" // 设置默认角色
             };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return ApiResponse.Success<object>(null, "注册成功");
        }
        catch (Exception ex)
        {
            return ApiResponse.Fail<object>($"注册失败: {ex.Message}");
        }
    }

    public async Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request)
{
    try
    {
        var user = await _context.Users.FirstOrDefaultAsync(u =>
            u.Username == request.Account || u.Email == request.Account);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return ApiResponse.Fail<LoginResponse>("用户名或密码错误");
        }

         // 用户验证通过后，生成 JWT Token
        var token = GenerateJwtToken(user.Id.ToString(), "Online");

        var response = new LoginResponse
        {
            User = new UserInfo
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email
            },
            Token = token
        };

        return ApiResponse.Success(response, "登录成功");
    }
    catch (Exception ex)
    {
        return ApiResponse.Fail<LoginResponse>($"登录失败: {ex.Message}");
    }
}

    public async Task<(bool success, string message)> ValidateUserAsync(string username, string password)
{
    try
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == username || u.Email == username);

        if (user == null)
        {
            return (false, "用户不存在");
        }

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            return (false, "密码错误");
        }

        if (!user.IsActive)
        {
            return (false, "账户已停用");
        }

        return (true, "验证成功");
    }
    catch (Exception ex)
    {
        _logger.LogError($"ValidateUser error: {ex.Message}", ex);
        return (false, "验证过程发生错误");
    }
}

    private bool VerifyPassword(string password, string storedHash, string storedSalt)
    {
        try
        {
            // 将存储的盐转换回字节数组
            byte[] saltBytes = Convert.FromBase64String(storedSalt);

            // 使用相同的方式加密输入的密码
            using (var hmac = new HMACSHA512(saltBytes))
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                string computedHashString = Convert.ToBase64String(computedHash);

                // 比较计算得到的哈希值和存储的哈希值
                return computedHashString == storedHash;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Password verification error: {ex.Message}");
            return false;
        }
    }

   public async Task<ApiResponse<bool>> SendVerificationCodeAsync(SendVerificationCodeRequest request)
{
    try
    {
        // 生成6位随机验证码
        string verificationCode = GenerateVerificationCode();

        // 存储验证码到Redis，设置5分钟过期
        string cacheKey = $"verification_code:{request.Email}";
        await _cache.SetStringAsync(cacheKey, verificationCode, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
        });

        // 发送验证码邮件
        await _emailService.SendVerificationCodeAsync(request.Email, verificationCode);

        return ApiResponse.Success(true, "验证码已发送");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "发送验证码失败");
        return ApiResponse.Fail<bool>("发送验证码失败", "SEND_CODE_ERROR");
    }
}

    public async Task<ApiResponse<bool>> VerifyCodeAsync(VerifyCodeRequest request)
    {
        try
        {
            // 从Redis获取存储的验证码
            string cacheKey = $"verification_code:{request.Email}";
            string storedCode = await _cache.GetStringAsync(cacheKey);

            if (string.IsNullOrEmpty(storedCode))
            {
                return ApiResponse.Fail<bool>("验证码已过期", "CODE_EXPIRED");
            }

            if (storedCode != request.Code)
            {
                return ApiResponse.Fail<bool>("验证码不正确", "INVALID_CODE");
            }

            // 验证成功后删除验证码
            await _cache.RemoveAsync(cacheKey);

            return ApiResponse.Success(true, "验证成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "验证码验证失败");
            return ApiResponse.Fail<bool>("验证失败", "VERIFY_ERROR");
        }
    }

    private string GenerateVerificationCode()
    {
        Random random = new Random();
        return random.Next(100000, 999999).ToString();
    }

    public string GenerateJwtToken(string userId, string role)
    {
        var key = Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]);
        var claims = new[]
        {
            new Claim("sub", userId),  // 用户 ID
            new Claim("role", role)   // 用户角色
        };

        var token = new JwtSecurityToken(
            expires: DateTime.UtcNow.AddHours(48),  // 设置有效时间，比如为 1 小时
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256),
            claims: claims
        );

        return new JwtSecurityTokenHandler().WriteToken(token); // 返回生成的 JWT 字符串
    }

    public async Task<User> GetUserByWeChatOpenId(string openId)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.WeChatOpenId == openId);
    }

    public async Task<User> BindWeChat(int userId, string openId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            throw new Exception("用户不存在");
        }

        // 检查是否已被其他用户绑定
        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.WeChatOpenId == openId);
        if (existingUser != null)
        {
            throw new Exception("该微信账号已被其他用户绑定");
        }

        user.WeChatOpenId = openId;
        user.LoginType = "WeChat";
        await _context.SaveChangesAsync();

        return user;
    }

    public async Task<User> Register(string username, string email, string password)
    {
        if (await _context.Users.AnyAsync(u => u.Email == email))
        {
            throw new Exception("邮箱已被注册");
        }

        var newUser = new User
        {
            Username = username,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            Role = "User"
        };

        _context.Users.Add(newUser);
        await _context.SaveChangesAsync();
        return newUser;
    }

    public async Task<string> Login(string email, string password)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            throw new Exception("用户名或密码错误");
        }

        return GenerateJwtToken(user.Id.ToString(), user.Role);
    }

    public async Task<User> GetUserById(int id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async Task<bool> ChangePassword(int userId, string oldPassword, string newPassword)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            throw new Exception("用户不存在");
        }

        if (!BCrypt.Net.BCrypt.Verify(oldPassword, user.PasswordHash))
        {
            throw new Exception("原密码错误");
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ResetPassword(string email, string newPassword)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null)
        {
            throw new Exception("用户不存在");
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<User> GetUserByEmail(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }
}

}