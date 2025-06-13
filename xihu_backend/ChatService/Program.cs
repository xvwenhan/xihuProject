using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ChatService.Services;
using ChatService.Data;
using MongoDB.Bson;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);
var key = Encoding.UTF8.GetBytes(builder.Configuration["JwtSettings:SecretKey"]);

// 注册 AgentService 到 DI 容器
builder.Services.AddScoped<ChatWithAgentService>();
// 加载 appsettings.json 配置并绑定到 AppSettings 类
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));

// 注册 MongoDB 客户端和服务
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var databaseUrl = builder.Configuration["AppSettings:DatabaseUrl"];
    return new MongoClient(databaseUrl);
});

builder.Services.AddScoped<MongoDBContext>(); // 注册 MongoDB 服务


/*// 添加 MongoDB 配置
var mongoClient = new MongoClient("mongodb://root:123456@8.133.201.233:27017");
var database = mongoClient.GetDatabase("chat_db");

// 测试连接
try
{
    var collection = database.GetCollection<BsonDocument>("chat_logs");
    Console.WriteLine("成功连接到 MongoDB 数据库！");
}
catch (Exception ex)
{
    Console.WriteLine($"连接失败: {ex.Message}");
}*/

// 添加 Swagger 服务
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
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


// 添加 Redis 缓存
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "";
});

builder.Services.AddAuthorization();
builder.Services.AddControllers();
var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

// 启用 Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.Run();

public class AppSettings
{
    public string AppKey { get; set; }
    public string AppSecret { get; set; }
    public string ModelId { get; set; }
    public string ModelId_New { get; set; }
    public string ModelId_GR {  get; set; }
    public string DatabaseUrl { get; set; }

}