
using System.Text.Json.Serialization;

namespace TerraMours_Gpt.Domains.GptDomain.Contracts.Req {
    /// <summary>
    /// AI聊天参数
    /// </summary>
    public class ChatReq {
        /// <summary>
        /// 提问词
        /// </summary>
        public string Prompt { get; set; }

        /// <summary>
        /// 会话id  新建会话传-1
        /// </summary>
        public long? ConversationId { get; set; }

        /// <summary>
        /// 系统提示词
        /// </summary>
        public string? SystemMessage { get; set; }
        /// <summary>
        /// 模型
        /// </summary>
        public string? Model { get; set; }
        /// <summary>
        /// 模型类型
        /// </summary>
        public int? ModelType { get; set; }
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
    
}
