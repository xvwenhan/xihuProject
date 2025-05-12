using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using ChatService.Services;
using ChatService.DTOs;

[Route("api/chat")]
[ApiController]
public class ChatController : ControllerBase
{
    private readonly AgentService _agentService;
    private readonly RedisQuestionService _redisQuestionService;

    public ChatController(AgentService agentService, RedisQuestionService redisQuestionService)
    {
        _redisQuestionService = redisQuestionService;
        _agentService = agentService;
    }

    //�������巢����Ϣ�յ��ظ��������¼�����ݿ�
    [Authorize]
    [HttpPost("private/execute")]
    public async Task<IActionResult> ExecuteAgentAsync([FromBody] ChatService.DTOs.InputRequest request)
    {
        var userId = Request.Headers["X-User-Id"].FirstOrDefault();
        var role = Request.Headers["X-User-Role"].FirstOrDefault();
        var response = await _agentService.ExecuteAgentAsync(request, userId);
        return response.Success ? Ok(response) : BadRequest(response);

    }

    //�����û�id��ȡ�����¼
    [Authorize]
    [HttpGet("private/getChatLogs")]

    public async Task<IActionResult> GetChatLogsAsync(int page, int pageSize)
    {
        var userId = Request.Headers["X-User-Id"].FirstOrDefault();
        var response = await _agentService.GetChatLogs(userId, page, pageSize);
        return response.Success ? Ok(response) : BadRequest(response);
    }



    [HttpGet("public/guessAsk")]
    public async Task<IActionResult> GuessWhatYouWantToAsk()
    {
        var response = await _redisQuestionService.GetTopQuestionsAsync(5);
        return response.Success ? Ok(response) : BadRequest(response);

    }

}

