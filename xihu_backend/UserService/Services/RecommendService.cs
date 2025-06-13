using UserService.Services.Interfaces;
using UserService.DTOs;
using Shared.Responses;
namespace UserService.Services
{
    public class RecommendService : IRecommendService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<RecommendService> _logger;

        public RecommendService(HttpClient httpClient, ILogger<RecommendService> logger)
        {
            _httpClient = httpClient;
            _logger = logger; 
        }
        /// <summary>
        /// 通过Flask获取会议推荐结果
        /// </summary>
        public async Task<ApiResponse<RecommendResponse?>> GetRecommendationsAsync(string userId)
        {
            var request = new RecommendDTOs { UserId = userId };

            try
            {
                var response = await _httpClient.PostAsJsonAsync("http://8.133.201.233:5005/recommend_for_user", request);
                // var response = await _httpClient.PostAsJsonAsync("http://localhost:5005/recommend_for_user", request);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Flask 服务返回错误状态码: {StatusCode}", response.StatusCode);
                    return ApiResponse.Fail<RecommendResponse?>("Flask 服务返回错误状态码", response.StatusCode.ToString());
                }

                var result = await response.Content.ReadFromJsonAsync<RecommendResponse>();
                return ApiResponse.Success(result, "成功获取推荐结果！");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "调用 Flask 接口失败");
                return ApiResponse.Fail<RecommendResponse?>("Flask 服务返回错误状态码", "400");
            }
        }




    }
}
