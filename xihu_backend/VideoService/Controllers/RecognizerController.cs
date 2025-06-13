using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Text;
using VideoService.Services;
using VideoService.DTOs;
using Shared.Responses;

namespace VideoService.Controllers
{
    [ApiController]
    [Route("api/video")]
    public class RecognizerController : ControllerBase
    {
        private readonly IStreamService _manager;
        private readonly IStreamInfoService _streamInfoManager;

        public RecognizerController(IStreamService manager, IStreamInfoService streamInfoManager)
        {
            _manager = manager;
            _streamInfoManager = streamInfoManager;
        }

        [HttpPost("public/start")]
        public async Task<IActionResult> Start([FromBody] StartRequest req)
        {
            var response = await _manager.StartSessionAsync(req.MeetingId, req.RoomId);
/*            if (response.Success)
               await _streamInfoManager.UpdateLiveStatusAsync(req.MeetingId, Models.LiveStatus.LIVE);*/
            return response.Success ? Ok(response) : BadRequest(response);
        }

        [HttpPost("public/stop/{meetingId}")]
        public async Task<IActionResult> Stop(string meetingId)
        {
            var response = await _manager.StopSessionAsync(meetingId);
/*            if (response.Success)
                await _streamInfoManager.UpdateLiveStatusAsync(meetingId, Models.LiveStatus.ENDED);*/
            return response.Success ? Ok(response) : BadRequest(response);
        }


        [HttpGet("public/stream/{meetingId}")]
        public async Task Stream(string meetingId)
        {
            Response.ContentType = "text/event-stream";
            Response.Headers.Add("Cache-Control", "no-cache");

            if (!_manager.TryGetQueue(meetingId, out var queue))
            {
                await Response.WriteAsync("event: error\ndata: 会话未启动\n\n");
                await Response.Body.FlushAsync();
                return;
            }

            await _manager.HandleEventStream(Response, HttpContext.RequestAborted, queue);
        }

        [HttpGet("public/summary/{meetingId}")]
        public async Task<IActionResult> GetVideoSummaries(int meetingId)
        {
            var response = await _streamInfoManager.GetVideoSummariesByMeetingIdAsync(meetingId);

            if (response.Success)
            {
                var summaries = response.Data as List<VideoSummaryResponse>;
                return Ok(summaries);
            }
            else
            {
                return BadRequest(response);
            }
        }

        [HttpPost("public/info/upsert")]
        public async Task<IActionResult> InsertOrUpdateStream([FromQuery] int conferenceId, [FromQuery] string roomId, [FromQuery] string channelId)
        {
            var response = await _streamInfoManager.InsertOrUpdateStreamAsync(conferenceId, roomId, channelId);
            return response.Success ? Ok(response) : BadRequest(response);
        }

        [HttpGet("public/info/{conferenceId}")]
        public async Task<ActionResult<StreamDto>> GetStreamByConferenceId(int conferenceId)
        {
            var response = await _streamInfoManager.GetStreamByConferenceIdAsync(conferenceId);
            return response.Success ? Ok(response) : BadRequest(response);
        }

        [HttpGet("public/list")]
        public async Task<ActionResult<ApiResponse<List<StreamDto?>>>> GetAllStreams()
        {
            var response = await _streamInfoManager.GetAllStreamsAsync();
            return response.Success ? Ok(response) : BadRequest(response);
        }

    }



}
