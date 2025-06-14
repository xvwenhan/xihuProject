﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs;
using Shared.Responses;
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

        /// <summary>
        /// 获取所有会议地点信息
        /// </summary>
        [Authorize]
        [HttpGet("private/locations")]
        public async Task<IActionResult> GetAllMeetingLocations()
        {
            var response = await _meetingService.GetAllMeetingLocationsAsync();
            return response.Success ? Ok(response) : BadRequest(response);
        }

        /// 子系统间调用接口：获取会议信息
        /// </summary>
        [HttpGet("public/conference/{conferenceId}")]
        public async Task<ActionResult<ApiResponse<ConferenceDto?>>> GetConferenceDetails(int conferenceId)
        {
            var response = await _meetingService.GetConferenceDetailsByIdAsync(conferenceId);
            return response.Success ? Ok(response) : BadRequest(response);
        }

    }
}
