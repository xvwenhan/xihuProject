using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using ChatService.Services;
using ChatService.DTOs;

[Route("api/chat")]
[ApiController]
public class ChatController : ControllerBase
{
    private readonly ChatService.Services.ChatWithAgentService _agentService;

    public ChatController(ChatService.Services.ChatWithAgentService agentService)
    {
        _agentService = agentService;
    }

    /// <summary>
    /// 与智能体对话
    /// </summary>
    [Authorize]
    [HttpPost("private/chat")]
    public async Task<IActionResult> ChatWithAgent([FromBody] ChatService.DTOs.InputRequest request)
    {
        var userId = Request.Headers["X-User-Id"].FirstOrDefault();
        var response = await _agentService.ChatWithAgentAsync(request, userId);
        return response.Success ? Ok(response) : BadRequest(response);

    }

    /// <summary>
    /// 获取会话历史
    /// </summary>
    [Authorize]
    [HttpGet("private/getChatLogs")]
    public async Task<IActionResult> GetChatLogs(int page, int pageSize)
    {
        var userId = Request.Headers["X-User-Id"].FirstOrDefault();
        var response = await _agentService.GetChatLogsAsync(userId, page, pageSize);
        return response.Success ? Ok(response) : BadRequest(response);
    }

    /// <summary>
    /// 向redis中缓存Q&A
    /// </summary>
    [HttpPost("public/redis")]
    public async Task<IActionResult> StoreInRedis([FromBody] QARequest request)
    {
        var response = await _agentService.StoreInRedisAsync(request);
        return response.Success ? Ok(response) : BadRequest(response);
    }

    /// <summary>
    /// 从redis中语义匹配Q&A
    /// </summary>
    [HttpGet("public/redis")]
    public async Task<IActionResult> FetchFromRedis([FromQuery] string question)
    {
        var response = await _agentService.FetchFromRedisAsync(question);
        return response.Success ? Ok(response) : BadRequest(response);
    }

    /// <summary>
    /// 获取猜你想问
    /// </summary>
    [HttpGet("public/guessAsk")]
    public async Task<IActionResult> GuessWhatYouWantToAsk()
    {
        var response = await _agentService.GetTopQuestionsAsync(3);
        return response.Success ? Ok(response) : BadRequest(response);
    }

}

