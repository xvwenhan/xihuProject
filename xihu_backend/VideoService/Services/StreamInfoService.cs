using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using Shared.DTOs;
using Shared.Responses;
using VideoService.Data;
using VideoService.DTOs;
using VideoService.Models;

namespace VideoService.Services
{
    public class StreamInfoService : IStreamInfoService
    {
        private readonly HttpClient _httpClient;
        private readonly VideoDbContext _context;
        private readonly string _meetingServiceUrl;

        public StreamInfoService(IHttpClientFactory httpClientFactory, IConfiguration configuration, VideoDbContext context)
        {
            _httpClient = httpClientFactory.CreateClient();
            _context = context;
            _meetingServiceUrl = configuration.GetValue<string>("ServiceUrls:UserService");
        }

        /// <summary>
        /// 插入或更新某会议的直播信息
        /// </summary>
        public async Task<ApiResponse<object>> InsertOrUpdateStreamAsync(int conferenceId, string roomId, string channelId)
        {
            var stream = await _context.Streams.FindAsync(conferenceId);

            if (stream == null)
            {
                // 插入新记录
                stream = new Models.Stream
                {
                    ConferenceId = conferenceId,
                    RoomId = roomId,
                    LiveStatus = LiveStatus.NOT_STARTED,
                    ChannelId = channelId
                };
                _context.Streams.Add(stream);
                await _context.SaveChangesAsync();
                return ApiResponse.Success<object>(null, $"成功插入会议直播信息");
            }
            else
            {
                // 更新房间号
                stream.RoomId = roomId;
                stream.ChannelId = channelId;
                _context.Streams.Update(stream);
                await _context.SaveChangesAsync();
                return ApiResponse.Success<object>(null, $"成功更新会议直播信息");
            }
        }

        /// <summary>
        /// 根据会议 ID 获取 Stream 信息
        /// </summary>
        public async Task<ApiResponse<StreamDto?>> GetStreamByConferenceIdAsync(int conferenceId)
        {
            var stream = await _context.Streams.FindAsync(conferenceId);

            if (stream == null)
                return null;

            var result = new StreamDto
            {
                ConferenceId = stream.ConferenceId,
                ChannelId = stream.ChannelId,
                RoomId = stream.RoomId,
                LiveStatus = stream.LiveStatus
            };

            return ApiResponse.Success<StreamDto?>(result, $"成功获取会议直播信息");
        }

        /// <summary>
        /// 更新某会议的直播状态
        /// </summary>
        public async Task<bool> UpdateLiveStatusAsync(string conferenceId, LiveStatus newStatus)
        {
            if (!int.TryParse(conferenceId, out int parsedId))
            {
                return false;
            }
            var stream = await _context.Streams.FindAsync(parsedId);

            if (stream == null)
                return false;

            stream.LiveStatus = newStatus;
            _context.Streams.Update(stream);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// 获取所有会议的 Stream信息
        /// </summary>
        public async Task<ApiResponse<List<StreamDto>>> GetAllStreamsAsync()
        {
            var streams = await _context.Streams.ToListAsync();
            var streamDtos = new List<StreamDto>();

            foreach (var stream in streams)
            {
                // 获取会议详情
                var conferenceDetails = await GetConferenceDetailsFromApiAsync(stream.ConferenceId);

                if (conferenceDetails != null)
                {
                    var streamDto = new StreamDto
                    {
                        ConferenceId = stream.ConferenceId,
                        RoomId = stream.RoomId,
                        ChannelId = stream.ChannelId,
                        LiveStatus = stream.LiveStatus, // Assuming LiveStatus is an enum
                        ConferenceName = conferenceDetails.ConferenceName,
                        StartTime = conferenceDetails.StartTime,
                        EndTime = conferenceDetails.EndTime,
                        IsOnlyOffline = conferenceDetails.IsOnlyOffline
                    };

                    streamDtos.Add(streamDto);
                }
            }

            return ApiResponse.Success(streamDtos, "成功获取所有会议信息");
        }


        /// <summary>
        /// 跨子系统调用会议详情接口，获取某会议其他相关信息
        /// </summary>
        public async Task<ConferenceDto?> GetConferenceDetailsFromApiAsync(int conferenceId)
        {
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<ConferenceDto?>>
                ($"{_meetingServiceUrl}/api/user/public/conference/{conferenceId}");
            return response.Data;
        }


        public async Task<ApiResponse<object>> GetVideoSummariesByMeetingIdAsync(int meetingId)
        {
            if (meetingId <= 0)
                return ApiResponse.Fail<object>("无效的会议ID", "400");

            // 查询数据库中所有与 meetingId 相关的视频摘要
            var summaries = await _context.Set<VideoSummary>()
                .Where(v => v.MeetingId == meetingId)
                .ToListAsync();

            if (summaries == null || summaries.Count == 0)
                return ApiResponse.Fail<object>("没有找到对应的会议摘要", "404");

            // 将实体列表映射为 DTO 列表
            var responseSummaries = summaries.Select(v => new VideoSummaryResponse
            {
                Id = v.Id,
                MeetingId = v.MeetingId,
                StartTime = v.StartTime,
                EndTime = v.EndTime,
                OriginalText = v.OriginalText,
                Summary = v.Summary
            }).ToList();

            return ApiResponse.Success<object>(responseSummaries, "获取摘要成功");
        }


    }



}
