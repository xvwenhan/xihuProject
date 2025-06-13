using Shared.Responses;
using UserService.DTOs;
namespace UserService.Services.Interfaces
{
    public interface IRecommendService
    {
        Task<ApiResponse<RecommendResponse?>> GetRecommendationsAsync(string userId);
    }
}
