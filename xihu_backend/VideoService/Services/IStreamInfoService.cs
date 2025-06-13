using Shared.DTOs;
using Shared.Responses;
using System.Collections.Concurrent;
using VideoService.DTOs;
using VideoService.Models;

namespace VideoService.Services
{
    public interface IStreamInfoService
    {
        Task<ApiResponse<object>> InsertOrUpdateStreamAsync(int conferenceId, string roomId, string channelId);
        Task<ApiResponse<StreamDto?>> GetStreamByConferenceIdAsync(int conferenceId);
        Task<bool> UpdateLiveStatusAsync(string conferenceId, LiveStatus newStatus);
        Task<ApiResponse<List<StreamDto>>> GetAllStreamsAsync();
        Task<ConferenceDto?> GetConferenceDetailsFromApiAsync(int conferenceId);
        Task<ApiResponse<object>> GetVideoSummariesByMeetingIdAsync(int meetingId);

    }

}