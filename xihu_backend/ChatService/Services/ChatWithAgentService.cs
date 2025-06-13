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
    public class ChatWithAgentService
    {
        private static readonly HttpClient client = new();
        private readonly MongoDBContext _mongoDBContext;
        private readonly AppSettings _appSettings;
        private readonly IDistributedCache _cache;
        private readonly ILogger<ChatWithAgentService> _logger;
        private readonly IConfiguration _config;

        public ChatWithAgentService(
            MongoDBContext mongoDBContext, 
            IOptions<AppSettings> appSettings,
            ILogger<ChatWithAgentService> logger,
            IDistributedCache cache,
            IConfiguration config)
        {
            _mongoDBContext = mongoDBContext;
            _appSettings = appSettings.Value;
            _logger = logger;
            _cache = cache;
            _config = config;
        }

        /**********************************************/
        /********************工具函数*******************/
        /**********************************************/

        private string GenerateSign(string key, string secret)
        {
            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            string data = $"{timestamp}\n{secret}\n{key}";
            using (var hmacsha256 = new HMACSHA256(Encoding.UTF8.GetBytes(secret)))
            {
                byte[] hashmessage = hmacsha256.ComputeHash(Encoding.UTF8.GetBytes(data));
                return $"{timestamp}{Convert.ToBase64String(hashmessage)}";
            }
        }

        private void SetHeaders()
        {
            // 使用 AppKey
            var appKey = _appSettings.AppKey;
            var appSecret = _appSettings.AppSecret;

            var sign = GenerateSign(appKey, appSecret);

            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("appKey", appKey);
            client.DefaultRequestHeaders.Add("sign", sign);
        }

        // 获取Redis中所有键（模拟StackExchange.Redis的Keys方法）
        private async Task<IEnumerable<string>> GetAllKeysAsync()
        {
            var keysSet = "question_keys";
            var keysJson = await _cache.GetStringAsync(keysSet);
            if (string.IsNullOrEmpty(keysJson))
            {
                return new List<string>();
            }
            return JsonSerializer.Deserialize<List<string>>(keysJson);
        }

        // 存会话历史到mongoDB
        private async Task<bool> StoreToDBAsync(ChatResult res, string uid)
        {
            // 创建新的 JSON 对象，包含 data、tid 和当前时间
            var newJsonObject = new
            {
                ChatResult = res,
                userId = uid
            };
            var bsonDocument = BsonDocument.Parse(JsonSerializer.Serialize(newJsonObject));

            // 插入到 MongoDB
            await _mongoDBContext.InsertDocumentAsync("chat_logs", bsonDocument);
            Console.WriteLine("数据已成功存储到 MongoDB！");
            return true;
        }

        // 判断问题是否是通用性的问题，使用GR智能体
        public async Task<bool> GRAsync(InputRequest request)
        {
            // 自动生成UUID格式的唯一会话ID
            var sid = Guid.NewGuid().ToString();
            var passinreq = new AgentExecuteRequest
            {
                id = _appSettings.ModelId_GR,
                sid = sid,
                input = request.input
            };
            SetHeaders();

            var jsonContent = JsonSerializer.Serialize(passinreq);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            // 向模型发送问题
            var response = await client.PostAsync("https://www.das-ai.com/open/api/v2/agent/execute", content);
            var temp = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiJResponse>(temp);

            // 获取 content 字符串（是一个 JSON 格式字符串）
            var contentJson = apiResponse?.data?.session?.messages?[1]?.content;
            Console.WriteLine($"contentJson = {contentJson}");
            foreach (var c in contentJson)
            {
                Console.WriteLine($"Character: {c}, Unicode: {(int)c}");
            }

            if (string.IsNullOrWhiteSpace(contentJson))
                return false;

            // 再次反序列化 content 字符串
            using var doc = JsonDocument.Parse(contentJson);
            if (doc.RootElement.TryGetProperty("isCommon", out var isCommonProp))
            {
                return isCommonProp.GetBoolean();
            }

            // 有什么问题一概返回false防止存储个性化QA
            return false;
        }


        /**********************************************/
        /********************接口函数*******************/
        /**********************************************/

        /// <summary>
        /// 与智能体对话
        /// </summary>
        public async Task<ApiResponse<ChatResult>> ChatWithAgentAsync(InputRequest request,string userId)
        {
            var response = new ChatResult();
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
                // 向智能体发送消息的准备工作，设置ID，headers等
                var sid = Guid.NewGuid().ToString();
                var passinreq = new AgentExecuteRequests
                {
                    id = _appSettings.ModelId_New,
                    sid = sid,
                    inputs = new Dictionary<string, string>
                        {
                            { "input", request.input },
                            { "userId", userId }
                        }
                 };

                SetHeaders();

                var jsonContent = JsonSerializer.Serialize(passinreq);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                //向智能体发送问题
                var httpResponse = await client.PostAsync("https://www.das-ai.com/open/api/v2/agent/execute", content);
                var temp = await httpResponse.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiJResponse>(temp);

                // 得到智能体的回答
                response.answer = apiResponse.data.session.messages[1].content;
                response.question = request.input;
                response.source = "agent";
                response.time= DateTime.Now.ToString("yyyy年M月d日 HH:mm");

                //通过另一个智能体判断该问题是否通用
                var commen = await GRAsync(new InputRequest { input = request.input });
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
            // 将问题存入MongoDB中，保存对话记录
            StoreToDBAsync(response, userId);
            return ApiResponse.Success<ChatResult>(response, "未存入缓存");

        }

        /// <summary>
        /// 获取会话历史
        /// </summary>
        public async Task<ApiResponse<PagedResult>> GetChatLogsAsync(string userId,int page,int pageSize)
        {
            var result = await _mongoDBContext.GetChatHistoryPagedAsync(userId, page, pageSize);
            return ApiResponse.Success(result);
        }

        /// <summary>
        /// 向redis中缓存Q&A
        /// </summary>
        public async Task<ApiResponse<object>> StoreInRedisAsync(QARequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Question) || string.IsNullOrWhiteSpace(request.Answer))
                {
                    return ApiResponse.Fail<object>("问题和答案不能为空");
                }

                // 将问题转为特征键
                var key = $"{request.Question}";
                var value = JsonSerializer.Serialize(request);

                // 7h过期
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(7)
                };

                await _cache.SetStringAsync(key, value, options);

                // 将键添加到集合中（用于后续获取所有键）
                var keysSet = "question_keys";
                var existingKeysJson = await _cache.GetStringAsync(keysSet);
                var existingKeys = string.IsNullOrEmpty(existingKeysJson)
                    ? new List<string>()
                    : JsonSerializer.Deserialize<List<string>>(existingKeysJson);

                existingKeys.Add(key);
                await _cache.SetStringAsync(keysSet, JsonSerializer.Serialize(existingKeys));
                
                return ApiResponse.Success<object>(null, "存入成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "操作失败");
                return ApiResponse.Fail<object>($"操作失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 从redis中语义匹配Q&A
        /// </summary>
        public async Task<ApiResponse<QAResponse>> FetchFromRedisAsync(string question)
        {
            try
            {
                // 连接python
                var pythonUrl = _config["ServiceUrls:Semantic"];
                var requestUrl = $"{pythonUrl}/semantic_match?question={Uri.EscapeDataString(question)}";
                using var httpClient = new HttpClient();

                var response = await httpClient.GetAsync(requestUrl);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("调用Python服务失败，状态码：{Code}", response.StatusCode);
                    return ApiResponse.Fail<QAResponse>("Python 服务调用失败");
                }

                // 处理python结果
                var responseBody = await response.Content.ReadAsStringAsync();
                var jsonDoc = JsonDocument.Parse(responseBody);
                var root = jsonDoc.RootElement;

                if (!root.GetProperty("success").GetBoolean())
                {
                    return ApiResponse.Fail<QAResponse>("未命中缓存");
                }

                var data = root.GetProperty("data");
                var result = new QAResponse
                {
                    Question = data.GetProperty("question").GetString() ?? "",
                    Answer = data.GetProperty("answer").GetString() ?? ""
                };

                return ApiResponse.Success(result, "来自 Python 服务");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "调用 Python 服务失败");
                return ApiResponse.Fail<QAResponse>($"调用 Python 服务失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取猜你想问
        /// </summary>
        public async Task<ApiResponse<List<string>>> GetTopQuestionsAsync(int count = 3)
        {
            try
            {
                // 获取 Redis 中所有键
                var serverKeys = await GetAllKeysAsync();

                // 过滤出非空的问题相关的键
                var questionKeys = serverKeys.Where(k => !string.IsNullOrWhiteSpace(k)).ToList();

                // 如果键的数量小于需要的数量，直接返回所有键
                if (questionKeys.Count <= count)
                    return ApiResponse.Success(questionKeys);

                // 随机打乱键的顺序并取前count条
                var random = new Random();
                var randomKeys = questionKeys.OrderBy(_ => random.Next()).Take(count).ToList();

                // 获取对应的值
                var questions = new List<string>();
                foreach (var key in randomKeys)
                {
                    questions.Add(key); 
                }

                return ApiResponse.Success(questions);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"获取 Redis 数据失败: {ex.Message}");
                return ApiResponse.Fail<List<string>>("获取 Redis 数据失败");
            }
        }


    }
}