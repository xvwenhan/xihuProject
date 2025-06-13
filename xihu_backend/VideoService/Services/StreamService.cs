using System.Collections.Concurrent;
using System.Net.Http;
using System.Text;
using Microsoft.CognitiveServices.Speech.Transcription;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Shared.DTOs;
using Shared.Responses;
using VideoService.Data;
using VideoService.DTOs;
using VideoService.Models;

namespace VideoService.Services
{
    public class StreamService : IStreamService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<StreamService> _logger;
        private readonly ConcurrentDictionary<string, BlockingCollection<string>> _clientQueues = new();

        public StreamService(IHttpClientFactory httpClientFactory,ILogger<StreamService> logger)
        {
            _httpClient = httpClientFactory.CreateClient();
            _logger = logger;
        }

        public async Task<ApiResponse<object>> StartSessionAsync(string meetingId, string roomId)
        {
            if (string.IsNullOrEmpty(meetingId) || string.IsNullOrEmpty(roomId))
                return ApiResponse.Fail<object>("缺少参数", "400");

            if (_clientQueues.ContainsKey(meetingId))
            {
                return ApiResponse.Fail<object>("会话已存在", "400");
            }

            var content = new StringContent(
                $"{{\"meeting_id\": \"{meetingId}\", \"room_id\": \"{roomId}\"}}",
                System.Text.Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync("http://127.0.0.1:5006/start", content);
            // 解析JSON响应体
            var json = await response.Content.ReadAsStringAsync();
            var obj = System.Text.Json.JsonDocument.Parse(json);
            string msg = obj.RootElement.GetProperty("msg").GetString();
            if (!response.IsSuccessStatusCode)
            {
                _clientQueues.TryRemove(meetingId, out _);
                return ApiResponse.Fail<object>(msg + "，识别失败", "400");
            }
            var queue = new BlockingCollection<string>();
            _clientQueues[meetingId] = queue;
            _logger.LogInformation("启动识别成功: {meetingId}", meetingId);
            StartListeningPythonSSE(meetingId); // 开始监听 Python SSE 推送内容

            return ApiResponse.Success<object>(msg, "监听会议成功");
        }

        public async Task<ApiResponse<object>> StopSessionAsync(string meetingId)
        {
            if (!_clientQueues.ContainsKey(meetingId))
                return ApiResponse.Fail<object>("想中止的会话未找到", "400");

            // 修改此处，将meetingId拼接到URL中
            var response = await _httpClient.PostAsync($"http://127.0.0.1:5006/stop/{meetingId}", null);

            var json = await response.Content.ReadAsStringAsync();
            var obj = System.Text.Json.JsonDocument.Parse(json);
            string msg = obj.RootElement.GetProperty("msg").GetString();

            if (!response.IsSuccessStatusCode)
            {
                return ApiResponse.Fail<object>(msg + $"，中止{meetingId}会话失败", "400");
            }
            _clientQueues.TryRemove(meetingId, out _);
            _logger.LogInformation("成功停止识别: {meetingId}", meetingId);
            return ApiResponse.Success<object>(msg, $"成功中止会话{meetingId}");
        }
        public bool TryGetQueue(string meetingId, out BlockingCollection<string> queue)
        {
            return _clientQueues.TryGetValue(meetingId, out queue);
        }

        public async Task HandleEventStream(HttpResponse response, CancellationToken token, BlockingCollection<string> queue)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (queue.TryTake(out var message, TimeSpan.FromMinutes(10)))
                    {
                        await response.WriteAsync(message);
                    }
                    else
                    {
                        await response.WriteAsync("event: ping\ndata: keepalive\n\n");
                    }
                    await response.Body.FlushAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "SSE 发送失败");
                    break;
                }
            }

        }

        public void StartListeningPythonSSE(string meetingId)
        {
            var url = $"http://127.0.0.1:5006/stream/{meetingId}";
            var queue = _clientQueues[meetingId];

            Task.Run(async () =>
            {
                try
                {
                    using var request = new HttpRequestMessage(HttpMethod.Get, url);
                    using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                    using var stream = await response.Content.ReadAsStreamAsync();
                    using var reader = new StreamReader(stream);

                    var sb = new StringBuilder();

                    while (!reader.EndOfStream)
                    {
                        var line = await reader.ReadLineAsync();

                        if (string.IsNullOrWhiteSpace(line))
                        {
                            var message = sb.ToString();
                            if (!string.IsNullOrWhiteSpace(message))
                            {
                                queue.Add(message + "\n\n"); // push to BlockingCollection
                                _logger.LogInformation("Received SSE: {0}", message.Trim());
                                sb.Clear();
                            }
                        }
                        else
                        {
                            sb.AppendLine(line);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while listening to Python SSE for meetingId: {0}", meetingId);
                }
            });
        }

         // 获取指定会议的所有视频摘要

    }



}
