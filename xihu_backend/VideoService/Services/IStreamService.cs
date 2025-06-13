using Shared.DTOs;
using Shared.Responses;
using System.Collections.Concurrent;
using VideoService.DTOs;
using VideoService.Models;

namespace VideoService.Services
{
    public interface IStreamService
    {
        Task<ApiResponse<object>> StartSessionAsync(string meetingId, string roomId);
        Task<ApiResponse<object>> StopSessionAsync(string meetingId);
        bool TryGetQueue(string meetingId, out BlockingCollection<string> queue);
        Task HandleEventStream(HttpResponse response, CancellationToken token, BlockingCollection<string> queue);
    }

}
