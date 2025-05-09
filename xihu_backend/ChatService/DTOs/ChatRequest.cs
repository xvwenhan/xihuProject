namespace ChatService.DTOs
{
    public class InputRequest
    {
        public string input { get; set; }
    }
    public class AgentExecuteRequest
    {
        public string id { get; set; }
        public string input { get; set; }
        public string sid { get; set; } // 可选，如果为空则自动生成
    }
    public class AgentExecuteRequests
    {
        public string id { get; set; }
        public Dictionary<string, string> inputs { get; set; }  // 更新为 Dictionary 来存储多个输入
        public string sid { get; set; }  // 可选，如果为空则自动生成
    }

    public class ChatResult
    {
        public string time { get; set; }
        public string source { get; set; }
        public string question { get; set; }
        public string answer { get; set; }
    }

    // 传入缓存&从缓存中取出
    public class QARequest
    {
        public string Question { get; set; }
        public string Answer { get; set; }
    }

}
