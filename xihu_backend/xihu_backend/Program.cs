using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
// ��� Swagger ����
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// **�ֶ����� ocelot.json**
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);
// **JWT ����**
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


// **��� CORS ����**
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.WithOrigins("http://localhost:5173") // ����ǰ�˷���
                  .AllowAnyMethod()                     // �������� HTTP ����
                  .AllowAnyHeader()                     // �������� Header
                  .AllowCredentials();                  // ���� Cookie����Ȩͷ��
        });
});


var app = builder.Build();

// **���� CORS�������� `UseAuthorization` ֮ǰ��**
app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

// **�Զ����м�������� JWT ��ע�� X-User-Id �� X-User-Role**
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
// ���� Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseOcelot().Wait();

app.Run();
