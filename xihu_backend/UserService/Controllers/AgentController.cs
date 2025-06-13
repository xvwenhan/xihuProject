using Microsoft.AspNetCore.Mvc;
using UserService.Services;
using UserService.Services.Interfaces;
using static UserService.DTOs.AgentDTOs;

///智能体接口不加jwt
namespace UserService.Controllers
{
    [ApiController]
    [Route("api/user")]
    public class AgentController : ControllerBase
    {
        private readonly IMeetingService _meetingService;
        private readonly IRecommendService _recommendService;

        public AgentController(IMeetingService meetingService,IRecommendService recommendService)
        {
            _meetingService = meetingService;
            _recommendService = recommendService;
        }

        /// <summary>
        /// 按时间升序返回用户订阅列表
        /// （路径传参测试失败，换成查询参数，成功）
        /// </summary>
        [HttpGet("public/subscribe")]
        public async Task<IActionResult> GetSubscribeMeetings([FromQuery]string userId)
        {
            var response = await _meetingService.SubscribeMeetingsAsync(userId);
            return response.Success ? Ok(response) : BadRequest(response);
        }

        /// <summary>
        /// 按时间升序返回用户订阅列表(不传参测试，成功）
        /// </summary>
        [HttpGet("public/subscribe/noUserId")]
        public async Task<IActionResult> GetSubscribeMeetings()
        {
            var response = await _meetingService.SubscribeMeetingsAsync("1");
            return response.Success ? Ok(response) : BadRequest(response);
        }

        /// <summary>
        /// 用户个性化推荐
        /// </summary>
        [HttpGet("public/recommend")]
        public async Task<IActionResult> Recommend([FromQuery] string userId)
        {
            var meetings = await _recommendService.GetRecommendationsAsync(userId);
            var response = await _meetingService.GetMeetingsByIdsAsync(meetings.Data, userId);
            return response.Success ? Ok(response) : BadRequest(response);
        }
    }
}
