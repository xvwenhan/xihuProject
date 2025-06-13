using Microsoft.AspNetCore.Mvc;
using UserService.DTOs;
using UserService.Services.Interfaces;

namespace UserService.Controllers
{
    [ApiController]
    [Route("api/user")]
    public class RecommendController : ControllerBase
    {
        private readonly IRecommendService _recommendService;
        private readonly IMeetingService _meetingService;

        public RecommendController(IRecommendService recommendService, IMeetingService meetingService)
        {
            _recommendService = recommendService;
            _meetingService = meetingService;
        }

        /// <summary>
        /// 获取用户的个性会议推荐结果
        /// </summary>
        [HttpPost("private/recommend")]
        public async Task<IActionResult> Recommend()
        {
            var userId = Request.Headers["X-User-Id"].FirstOrDefault();
            var meetings = await _recommendService.GetRecommendationsAsync(userId);
            var response = await _meetingService.GetMeetingsByIdsAsync(meetings.Data,userId);

            return response.Success ? Ok(response) : BadRequest(response);
        }
    }
}
