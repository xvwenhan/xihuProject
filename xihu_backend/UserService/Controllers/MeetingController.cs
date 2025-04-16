using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserService.DTOs;
using UserService.Services;
using UserService.Services.Interfaces;
using static UserService.DTOs.MeetingDTOs;

namespace UserService.Controllers
{
    [ApiController]
    [Route("api/user")]
    public class MeetingController:ControllerBase
    {
        private readonly IMeetingService _meetingService;

        public MeetingController(IMeetingService meetingService)
        {
            _meetingService = meetingService;
        }

        /// <summary>
        /// 订阅/取消订阅会议
        /// </summary>
        [Authorize]
        [HttpPost("private/subscribe")]
        public async Task<IActionResult> Subscribe([FromBody] SubscribeRequest request)
        {
            var userId = Request.Headers["X-User-Id"].FirstOrDefault();
            var response = await _meetingService.SubscribeAsync(request, userId);
            return response.Success ? Ok(response) : BadRequest(response);
        }

        /// <summary>
        /// 按一定顺序返回会议列表
        /// </summary>
        [Authorize]
        [HttpGet("private/sort")]
        public async Task<IActionResult> SortMeetings([FromQuery] SortRequest request)
        {
            var userId = Request.Headers["X-User-Id"].FirstOrDefault();
            var response = await _meetingService.SortMeetingsAsync(request,userId);
            return response.Success ? Ok(response) : BadRequest(response);
        }

        /// <summary>
        /// 按时间升序返回用户订阅列表
        /// </summary>
        [Authorize]
        [HttpGet("private/subscribe")]
        public async Task<IActionResult> GetSubscribeMeetings()
        {
            var userId = Request.Headers["X-User-Id"].FirstOrDefault();
            var response = await _meetingService.SubscribeMeetingsAsync(userId);
            return response.Success ? Ok(response) : BadRequest(response);
        }

    }
}
