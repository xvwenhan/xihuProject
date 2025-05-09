using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Shared.Responses;
using System.Security.Cryptography;
using ChatService.DTOs;
using ChatService.Data;
using System.Text.Json;
using MongoDB.Bson;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Caching.Distributed;

namespace ChatService.Services
{
    public class AgentService
    {
        private static readonly HttpClient client = new();
        private readonly MongoDBContext _mongoDBContext;
        private readonly AppSettings _appSettings;
        private readonly ILogger<AgentService> _logger;
        private readonly IDistributedCache _cache;
        private readonly IConfiguration _config;

        public AgentService(
            MongoDBContext mongoDBContext, 
            IOptions<AppSettings> appSettings,
            ILogger<AgentService> logger,
            IDistributedCache cache,
            IConfiguration config)
        {
            _mongoDBContext = mongoDBContext;
            _appSettings = appSettings.Value;
            _logger = logger;
            _cache = cache;
            _config = config;
        }

        public async Task<ApiResponse<ChatResult>> ExecuteAgentAsync(InputRequest request,string userId)
        {
            var response = new ChatResult();
            Console.WriteLine($"传给python的参数: {request.input}");
            //首先检测redis中是否有了类似问题，有就返回答案
            var similarQA = await FetchFromRedisAsync(request.input);
            if (similarQA.Success)
            {
                response.answer = similarQA.Data.Answer;
                response.question = similarQA.Data.Question;
                response.source = "redis";
                response.time = DateTime.Now.ToString("yyyy年M月d日 HH:mm");
            }
            else{
                // 自动生成UUID格式的唯一会话ID
                var sid = Guid.NewGuid().ToString();
                var passinreq = new AgentExecuteRequests
                {
                    id = _appSettings.ModelId_New,
                    sid = sid,
                    inputs = request.input ,

                 };

                SetHeaders();

                var jsonContent = JsonSerializer.Serialize(passinreq);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                //向模型发送问题
                var httpResponse = await client.PostAsync("https://www.das-ai.com/open/api/v2/agent/execute", content);
                var temp = await httpResponse.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiJResponse>(temp);

              
                response.answer = apiResponse.data.session.messages[1].content;
                response.question = request.input;
                response.source = "agent";
                response.time= DateTime.Now.ToString("yyyy年M月d日 HH:mm");

                //判断问题是否通用
                var commen = await GRAsync(new InputRequest { input = request.input });
                Console.WriteLine($"问题是否通用: {commen}");
                if (commen)
                {
                    //是通用的存到redis中
                    await StoreInRedisAsync(new QARequest
                    {
                        Question = request.input,
                        Answer = response.answer
                    });
                    StoreToDBAsync(response, userId);
                    return ApiResponse.Success<ChatResult>(response, "通用问题，存入缓存");
                }
            }
            Console.WriteLine($"准备存入mongodb");
            StoreToDBAsync(response, userId);
            return ApiResponse.Success<ChatResult>(response, "未存入缓存");

        }
        
        public async Task<ApiResponse<PagedResult>> GetChatLogs(string userId,int page,int pageSize)
        {
            var result = await _mongoDBContext.GetChatHistoryPagedAsync(userId, page, pageSize);
            return ApiResponse.Success(result);
        }

        private void SetHeaders()
        {
            // 使用 AppKey
            var appKey = _appSettings.AppKey;
            var appSecret= _appSettings.AppSecret;

            var sign = GenerateSign(appKey, appSecret);

            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("appKey", appKey);
            client.DefaultRequestHeaders.Add("sign", sign);
        }



    }
}