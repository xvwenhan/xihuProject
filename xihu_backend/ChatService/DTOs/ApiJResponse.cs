using MongoDB.Bson;
namespace ChatService.DTOs
{
    //模型用量
    public class TokenUsage
    {
        public int completion_tokens { get; set; } //生成token
        public int prompt_tokens { get; set; } //输入token
        public completion_tokens_details? completion_tokens_details { get; set; } //
        public int total_tokens { get; set; } //总token
    }

    public class completion_tokens_details
    {
        public int reasoning_tokens { get; set; }
    }

    //会话记录
    public class Message
    {
        public string? role { get; set; } //消息角色
        public string? content { get; set; } //消息内容
    }

    //会话信息
    public class session
    {
        public List<Message>? messages { get; set; } //会话记录
        public string? id { get; set; } //会话id
    }

    //智能体执行结果
    public class AgentResult
    {
        public TokenUsage token_usage { get; set; } //模型用量
        public session session { get; set; }  //会话信息
        public object results { get; set; }  //智能体解析信息
    }

    //智能体响应构成
    public class ApiJResponse
    {
        public string? msg { get; set; }  //消息
        public int code { get; set; }    //状态码，0成功，非0失败
        public int? flag { get; set; }    //状态码，0成功
        public AgentResult? data { get; set; }   //智能体执行结果
        public string? tid { get; set; }   //线程编号？没有用
    }

    // 分页结果模型
    public class PagedResult
    {
        public long TotalCount { get; set; } // 总记录数
        public int Page { get; set; }        // 当前页码
        public int PageSize { get; set; }    // 每页记录数
        public string userId { get; set; }
        public List<ChatLog>? Data { get; set; } // 当前页的数据
    }
    public class ChatLog
    {
        public DateTime timestamp { get; set; }
        public string question { get; set; }
        public string answer { get; set; }
    }

    // 从缓存中取出
    public class QAResponse
    {
        public string Question { get; set; }
        public string Answer { get; set; }
    }
}
