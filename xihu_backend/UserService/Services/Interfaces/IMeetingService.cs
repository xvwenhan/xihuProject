using System.Threading.Tasks;
using UserService.DTOs;
using Shared.Responses;
using UserService.Services;
using static UserService.DTOs.MeetingDTOs;
using Shared.DTOs;

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

         /// <summary>
        /// 获取会议地址
        /// </summary>
        Task<ApiResponse<List<MeetingLocationResponse>>> GetAllMeetingLocationsAsync();

        /// <summary>
        /// 获取某些会议的基本信息
        /// </summary>
        Task<ApiResponse<List<MeetingResponse>>> GetMeetingsByIdsAsync(RecommendResponse meetingIds, string userId);

        /// <summary>
        /// 获取会议列表（简洁版）
        /// </summary>
        /// <param name="request"></param>
        Task<ApiResponse<List<EasyMeetingResponse>>> GetMeetingsAsync();

        /// <summary>
        /// 子系统间调用：获取某一会议的基本信息
        /// </summary>
        Task<ApiResponse<ConferenceDto?>> GetConferenceDetailsByIdAsync(int conferenceId);

    }
}
