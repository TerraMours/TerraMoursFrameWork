using Newtonsoft.Json;

namespace TerraMours_Gpt.Domains.GptDomain.Contracts.Req {
    /// <summary>
    /// AI聊天参数
    /// </summary>
    /// <param name="Prompt">提问词</param>
    /// <param name="Options">配置</param>
    public record ChatReq(string Prompt, ChatOptions Options) {
        [JsonIgnore]
        public string? Key { get; set; }
        [JsonIgnore]
        public string? IP { get; set; }
    }
    /// <summary>
    /// 配置
    /// </summary>
    /// <param name="ConversationId"></param>
    /// <param name="SystemMessage"></param>
    /// <param name="ChatModel"></param>
    public record ChatOptions(int? ConversationId, string? SystemMessage,string? ChatModel);
}
