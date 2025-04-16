using System.Threading.Tasks;
using UserService.DTOs;
using Shared.Responses;
using UserService.Services;
using static UserService.DTOs.MeetingDTOs;

namespace UserService.Services.Interfaces
{
    public interface IMeetingService
    {

        /// <summary>
        /// 订阅or取消订阅会议
        /// </summary>
        Task<ApiResponse<object>> SubscribeAsync(SubscribeRequest request,string userId);

        /// <summary>
        /// 获取订阅列表（按时间升序）
        /// </summary>
        Task<ApiResponse<List<SubscribeResponse>>> SubscribeMeetingsAsync(String userId);

        /// <summary>
        /// 获取会议列表（按时间/热度，升序/降序）
        /// </summary>
        /// <param name="request"></param>
        Task<ApiResponse<List<MeetingResponse>>> SortMeetingsAsync(SortRequest request,string userId);

    }
}
