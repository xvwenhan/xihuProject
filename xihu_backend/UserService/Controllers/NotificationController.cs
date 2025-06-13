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
    public class NotificationController : ControllerBase
    {
        private readonly NotificationService _notificationService;

        public NotificationController(NotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [Authorize]
        [HttpGet("private/sse")]
        public async Task SseNotifications()
        {
            var userId = Request.Headers["X-User-Id"].FirstOrDefault();
            await _notificationService.SendSseNotifications(userId, Response);
        }

        ///// <summary>
        ///// 添加浏览器订阅推送
        ///// </summary>
        //[Authorize]
        //[HttpPost("private/subPush")]
        //public async Task<IActionResult> SubPush([FromBody] SubPush request)
        //{
        //    var userId = Request.Headers["X-User-Id"].FirstOrDefault();
        //    var response = await _notificationService.SubPushAsync(request, userId);
        //    return response.Success ? Ok(response) : BadRequest(response);
        //}
    }
   
}
