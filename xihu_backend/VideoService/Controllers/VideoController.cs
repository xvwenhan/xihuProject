using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Responses;
using VideoService.DTOs;
using VideoService.Services;
using Microsoft.Extensions.Logging;

namespace VideoService.Controllers
{
    /// <summary>
    /// 视频服务控制器
    /// 提供视频上传、转写、查询等功能
    /// </summary>
    [ApiController]
    [Route("api/video/private/[controller]")]
    [Authorize]
    public class VideoController : ControllerBase
    {
        private readonly IVideoService _videoService;
        private readonly ILogger<VideoController> _logger;

        public VideoController(IVideoService videoService, ILogger<VideoController> logger)
        {
            _videoService = videoService;
            _logger = logger;
        }

        /// <summary>
        /// 上传视频并进行语音识别
        /// </summary>
        /// <param name="uploadDTO">包含视频文件的上传数据</param>
        /// <returns>上传成功的视频信息</returns>
        /// <response code="200">视频上传成功</response>
        /// <response code="400">上传失败，返回错误信息</response>
        /// <response code="401">未授权访问</response>
        [HttpPost("upload")]
        [ProducesResponseType(typeof(ApiResponse<VideoDTO>), 200)]
        [ProducesResponseType(typeof(ApiResponse<VideoDTO>), 400)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<ApiResponse<VideoDTO>>> UploadVideo([FromForm] VideoUploadDTO uploadDTO)
        {
            try
            {
                var userId = int.Parse(User.Identity?.Name ?? "0");
                var result = await _videoService.UploadVideoAsync(uploadDTO, userId);
                return Ok(ApiResponse.Success(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Fail<VideoDTO>(ex.Message));
            }
        }

        /// <summary>
        /// 获取视频转写结果
        /// </summary>
        /// <param name="videoId">视频ID</param>
        /// <returns>视频的转写文本内容</returns>
        /// <response code="200">成功获取转写结果</response>
        /// <response code="400">获取失败，返回错误信息</response>
        /// <response code="401">未授权访问</response>
        /// <response code="404">视频不存在</response>
        /// <response code="409">视频转写尚未完成</response>
        [HttpGet("transcription/{videoId}")]
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(typeof(string), 400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(409)]
        public async Task<IActionResult> GetTranscription(int videoId)
        {
            try
            {
                var transcription = await _videoService.ProcessVideoTranscriptionAsync(videoId);
                // 直接返回转写文本内容，不包装在JSON中
                return Content(transcription, "text/plain; charset=utf-8");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取视频转写失败");
                return StatusCode(500, ex.Message);
            }
        }

        /// <summary>
        /// 获取视频处理状态
        /// </summary>
        /// <param name="id">视频ID</param>
        /// <returns>视频处理状态信息</returns>
        /// <response code="200">成功获取状态</response>
        /// <response code="400">获取失败，返回错误信息</response>
        /// <response code="401">未授权访问</response>
        /// <response code="404">视频不存在</response>
        [HttpGet("status/{id}")]
        [ProducesResponseType(typeof(ApiResponse<VideoProcessingStatusDTO>), 200)]
        [ProducesResponseType(typeof(ApiResponse<VideoProcessingStatusDTO>), 400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<ApiResponse<VideoProcessingStatusDTO>>> GetVideoStatus(int id)
        {
            try
            {
                var result = await _videoService.GetVideoStatusAsync(id);
                return Ok(ApiResponse.Success(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Fail<VideoProcessingStatusDTO>(ex.Message));
            }
        }

        /// <summary>
        /// 获取当前用户的所有视频列表
        /// </summary>
        /// <returns>用户的视频列表</returns>
        /// <response code="200">成功获取视频列表</response>
        /// <response code="401">未授权访问</response>
        [HttpGet("list")]
        [ProducesResponseType(typeof(ApiResponse<List<VideoDTO>>), 200)]
        [ProducesResponseType(401)]
        public async Task<ActionResult<ApiResponse<List<VideoDTO>>>> GetUserVideos()
        {
            try
            {
                var userId = int.Parse(User.Identity?.Name ?? "0");
                var videos = await _videoService.GetUserVideosAsync(userId);
                return Ok(ApiResponse.Success(videos));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Fail<List<VideoDTO>>(ex.Message));
            }
        }

        /// <summary>
        /// 删除指定的视频
        /// </summary>
        /// <param name="videoId">视频ID</param>
        /// <returns>删除操作结果</returns>
        /// <response code="200">视频删除成功</response>
        /// <response code="400">删除失败，返回错误信息</response>
        /// <response code="401">未授权访问</response>
        /// <response code="404">视频不存在</response>
        [HttpDelete("{videoId}")]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        [ProducesResponseType(typeof(ApiResponse<bool>), 400)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteVideo(int videoId)
        {
            try
            {
                var userId = int.Parse(User.Identity?.Name ?? "0");
                var result = await _videoService.DeleteVideoAsync(videoId, userId);
                return Ok(ApiResponse.Success(result));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse.Fail<bool>(ex.Message));
            }
        }
    }
} 