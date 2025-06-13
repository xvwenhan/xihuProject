using Microsoft.EntityFrameworkCore;
using UserService.Data;
using UserService.Models;
using UserService.DTOs;
using Shared.Responses;
using UserService.Services.Interfaces;
using UserService.Controllers;
using static UserService.DTOs.MeetingDTOs;
using Org.BouncyCastle.Asn1.Ocsp;
using Shared.DTOs;

namespace UserService.Services
{
    public class MeetingService : IMeetingService
    {
        private readonly UserDbContext _context;
        private readonly ILogger<MeetingService> _logger;

        public MeetingService(
            UserDbContext context,
            ILogger<MeetingService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ApiResponse<object>> SubscribeAsync(SubscribeRequest request, string userId)
        {
            try
            {
                // 转换成find支持的int型
                if (!int.TryParse(userId, out int parsedUserId))
                {
                    return ApiResponse.Fail<object>("用户ID格式错误", "400");
                }
                // 确认用户是否存在
                var user = await _context.Users.FindAsync(parsedUserId);
                if (user == null)
                {
                    return ApiResponse.Fail<object>("用户不存在", "USER_NOT_FOUND");
                }

                // 确认会议是否存在
                var conference = await _context.Conferences.FindAsync(request.ConferenceId);
                if (conference == null)
                {
                    return ApiResponse.Fail<object>("会议不存在", "CONFERENCE_NOT_FOUND");
                }

                // 线下热度处理
                var isOffline = user.Role == "Offline";

                // 检查用户是否已经订阅该会议
                var subscription = await _context.Subscribes
                    .FirstOrDefaultAsync(uc => uc.UserId == user.Id && uc.ConferenceId == request.ConferenceId);

                // 如果订阅了则取消订阅
                if (subscription != null)
                {
                    _context.Subscribes.Remove(subscription);
                    if (isOffline && conference.OfflineNum > 0)
                    {
                        conference.OfflineNum--;
                    }
                    conference.SubscribeNum--;
                    await _context.SaveChangesAsync();
                    return ApiResponse.Success<object>(null, "成功取消订阅");
                }

                // 创建新的订阅记录
                var newSubscription = new Subscribe
                {
                    UserId = user.Id,
                    ConferenceId = request.ConferenceId
                };
                _context.Subscribes.Add(newSubscription);
                if (isOffline)
                {
                    conference.OfflineNum++;
                }
                conference.SubscribeNum++;
                await _context.SaveChangesAsync();

                return ApiResponse.Success<object>(null, "订阅成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "操作失败");
                return ApiResponse.Fail<object>($"操作失败: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<SubscribeResponse>>> SubscribeMeetingsAsync(String userId)
        {
            try
            {
                // 将 userId 转换为 int 类型
                if (!int.TryParse(userId, out int parsedUserId))
                {
                    return ApiResponse.Fail<List<SubscribeResponse>>("用户ID格式错误", "400");
                }

                // 查询用户订阅的会议
                var subscriptions = await _context.Subscribes
                    .Where(s => s.UserId == parsedUserId)
                    .Include(s => s.Conference)
                    .OrderBy(s => s.Conference.Date)  //按时间升序
                    .ThenBy(s => s.Conference.StartTime)
                    .Select(s => new SubscribeResponse
                    {
                        Id = s.Conference.ConferenceId,
                        Name = s.Conference.ConferenceName,
                        Date = s.Conference.Date.ToString(),  // 格式化日期
                        Time = $"{s.Conference.StartTime.ToString()} ~ {s.Conference.EndTime.ToString()}",
                        Type = s.Conference.Type,
                        IsOnlyOffline = s.Conference.IsOnlyOffline == "是",
                        Location = s.Conference.Location,
                        SubscribeNum = s.Conference.SubscribeNum,
                        Url = s.Conference.Url
                    })
                    .ToListAsync();

                if (subscriptions == null || !subscriptions.Any())
                {
                    return ApiResponse.Fail<List<SubscribeResponse>>("没有找到订阅的会议", "NO_SUBSCRIPTIONS");
                }

                return ApiResponse.Success(subscriptions, "获取订阅会议列表成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "操作失败");
                return ApiResponse.Fail<List<SubscribeResponse>>($"操作失败: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<MeetingResponse>>> SortMeetingsAsync(SortRequest request, String userId)
        {
            try
            {
                // 查询会议信息
                var query = _context.Conferences.AsQueryable();

                // 排序四选一
                if (request.SortByTime)
                {
                    query = request.IsAsc
                        ? query.OrderBy(c => c.Date).ThenBy(c => c.StartTime)
                        : query.OrderByDescending(c => c.Date).ThenByDescending(c => c.StartTime);

                }
                else
                {
                    query = request.IsAsc
                        ? query.OrderBy(c => c.SubscribeNum)
                        : query.OrderByDescending(c => c.SubscribeNum);
                }

                if (!int.TryParse(userId, out int parsedUserId))
                {
                    return ApiResponse.Fail<List<MeetingResponse>>("无效的用户ID", "400");
                }

                // 获取用户已订阅的会议ID集合
                var subscribedConferenceIds = await _context.Subscribes
                    .Where(s => s.UserId == parsedUserId)
                    .Select(s => s.ConferenceId)
                    .ToListAsync();

                // 执行查询并转换为 MeetingResponse 进行返回
                var sortedMeetings = await query
                    .Select(c => new MeetingResponse
                    {
                        Id = c.ConferenceId,
                        Name = c.ConferenceName,
                        Time = c.Date.ToString() + " " + c.StartTime.ToString(),
                        Type = c.Type,
                        IsOnlyOffline = c.IsOnlyOffline == "是",
                        Location = c.Location,
                        OfflineNum = c.OfflineNum,
                        Url = c.Url,
                        IsSubscribed = subscribedConferenceIds.Contains(c.ConferenceId),
                        SubscribeNum = c.SubscribeNum
                    })
                    .ToListAsync();

                return ApiResponse.Success(sortedMeetings, "排序成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "会议排序失败");
                return ApiResponse.Fail<List<MeetingResponse>>($"会议排序失败: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<MeetingResponse>>> GetMeetingsByIdsAsync(RecommendResponse recommends, string userId)
        {
            try
            {
                if (!int.TryParse(userId, out int parsedUserId))
                {
                    return ApiResponse.Fail<List<MeetingResponse>>("无效的用户ID", "400");
                }
                List<int> meetingIds = recommends.Recommendations;
                if (meetingIds == null || meetingIds.Count == 0)
                {
                    // 前五热门会议
                    meetingIds = await _context.Conferences
                                .OrderByDescending(c => c.SubscribeNum)
                                .Take(5)
                                .Select(c => c.ConferenceId)
                                .ToListAsync();
                }

                // 查询指定 ID 的会议
                var query = _context.Conferences
                    .Where(c => meetingIds.Contains(c.ConferenceId));

                // 获取用户已订阅的会议 ID
                var subscribedConferenceIds = await _context.Subscribes
                    .Where(s => s.UserId == parsedUserId)
                    .Select(s => s.ConferenceId)
                    .ToListAsync();

                // 查询并转换为 MeetingResponse
                var meetings = await query
                    .Select(c => new MeetingResponse
                    {
                        Id = c.ConferenceId,
                        Name = c.ConferenceName,
                        Time = c.Date.ToString() + " " + c.StartTime.ToString(),
                        Type = c.Type,
                        IsOnlyOffline = c.IsOnlyOffline == "是",
                        Location = c.Location,
                        OfflineNum = c.OfflineNum,
                        Url = c.Url,
                        IsSubscribed = subscribedConferenceIds.Contains(c.ConferenceId),
                        SubscribeNum = c.SubscribeNum
                    })
                    .ToListAsync();

                return ApiResponse.Success(meetings, "查询成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "根据会议ID列表查询会议信息失败");
                return ApiResponse.Fail<List<MeetingResponse>>($"查询失败: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<MeetingLocationResponse>>> GetAllMeetingLocationsAsync()
        {
    try
    {
        // 查询所有会议地点
        var locations = await _context.MeetingLocations
            .OrderBy(l => l.Name)  // 按名称排序
            .Select(l => new MeetingLocationResponse
            {
                Id = l.Id,
                Name = l.Name,
                Latitude = l.Latitude,
                Longitude = l.Longitude,
                RouteFromAirport1 = l.RouteFromAirport1,
                RouteFromAirport2 = l.RouteFromAirport2
            })
            .ToListAsync();

        if (locations == null || !locations.Any())
        {
            return ApiResponse.Fail<List<MeetingLocationResponse>>("没有找到会议地点信息", "NO_LOCATIONS");
        }

        return ApiResponse.Success(locations, "获取会议地点列表成功");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "获取会议地点失败");
        return ApiResponse.Fail<List<MeetingLocationResponse>>($"获取会议地点失败: {ex.Message}");
    }
}

        public async Task<ApiResponse<List<EasyMeetingResponse>>> GetMeetingsAsync()
        {
            try
            {
                // 查询会议信息（不排序）
                var query = _context.Conferences.AsQueryable();

                // 执行查询并仅返回会议 ID 和名称
                var meetings = await query
                    .Select(c => new EasyMeetingResponse
                    {
                        Id = c.ConferenceId,
                        Name = c.ConferenceName
                    })
                    .ToListAsync();

                return ApiResponse.Success(meetings, "查询成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取会议信息失败");
                return ApiResponse.Fail<List<EasyMeetingResponse>>($"获取会议信息失败: {ex.Message}");
            }


        }

        public async Task<ApiResponse<ConferenceDto?>> GetConferenceDetailsByIdAsync(int conferenceId)
        {
            var conference = await _context.Conferences
                .Where(c => c.ConferenceId == conferenceId)
                .FirstOrDefaultAsync();

            if (conference == null)
            {
                return ApiResponse.Fail<ConferenceDto?>(null,"会议ID不存在或未找到相关会议");
            }

            var conferenceDto = new ConferenceDto
            {
                ConferenceId = conference.ConferenceId,
                ConferenceName = conference.ConferenceName,
                StartTime = conference.StartTime,
                EndTime = conference.EndTime,
                IsOnlyOffline = conference.IsOnlyOffline,
            };

            return ApiResponse.Success(conferenceDto, "成功获取会议详情");
        }


    }
}
