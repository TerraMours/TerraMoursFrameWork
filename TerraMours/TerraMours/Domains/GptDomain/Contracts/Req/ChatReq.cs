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

        /// <summary>
        /// 用户id 自增
        /// </summary>
        [JsonIgnore]
        public long UserId { get; set; }
        /// <summary>
        /// 角色id
        /// </summary>
        [JsonIgnore]
        public long? RoleId { get; set; }

    }
    /// <summary>
    /// 配置
    /// </summary>
    /// <param name="ConversationId">会话id</param>
    /// <param name="SystemMessage"></param>
    /// <param name="Model">模型</param>
    /// <param name="ModelType">模型类型</param>
    public record ChatOptions(int? ConversationId, string? SystemMessage,string? Model, int? ModelType);
}
