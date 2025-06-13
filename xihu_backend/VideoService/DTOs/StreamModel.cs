using VideoService.Models;

namespace VideoService.DTOs
{
    public class StartRequest
    {
        public string MeetingId { get; set; }
        public string RoomId { get; set; }
    }

    public class StreamDto
    {
        public int ConferenceId { get; set; }

        public string ConferenceName { get; set; }
        public string? RoomId { get; set; }
        public LiveStatus LiveStatus { get; set; }
        public string? ChannelId { get; set;}
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public string IsOnlyOffline { get; set; }
    }
}
