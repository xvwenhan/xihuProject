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

    }
   
}
