using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
// 添加 Swagger 服务
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// **手动加载 ocelot.json**
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
// **JWT 配置**
var key = Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"]);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer("Bearer", options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true
        };
    });

builder.Services.AddOcelot();


// **添加 CORS 配置**
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins("http://localhost:5173") // 允许前端访问
                  .AllowAnyMethod()                     // 允许所有 HTTP 方法
                  .AllowAnyHeader()                     // 允许所有 Header
                  .AllowCredentials();                  // 允许 Cookie、授权头等
        });
});


var app = builder.Build();

// **启用 CORS（必须在 `UseAuthorization` 之前）**
app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

// **自定义中间件：解析 JWT 并注入 X-User-Id 和 X-User-Role**
app.Use(async (context, next) =>
{
    if (context.Request.Headers.ContainsKey("Authorization"))
    {
        var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        var handler = new JwtSecurityTokenHandler();

        try
        {
            var jwtToken = handler.ReadJwtToken(token);
            var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            var role = jwtToken.Claims.FirstOrDefault(c => c.Type == "role")?.Value;

            if (userId != null)
                context.Request.Headers.Append("X-User-Id", userId);
            if (role != null)
                context.Request.Headers.Append("X-User-Role", role);
        }
        catch
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Invalid Token");
            return;
        }
    }

    await next();
});
// 启用 Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseOcelot().Wait();

app.Run();
