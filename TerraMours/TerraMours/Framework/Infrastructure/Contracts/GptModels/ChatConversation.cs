using TerraMours.Framework.Infrastructure.Contracts.SystemModels;

namespace TerraMours_Gpt.Framework.Infrastructure.Contracts.GptModels
{
    /// <summary>
    /// 聊天会话表
    /// </summary>
    public class ChatConversation:BaseEntity
    {
        /// <summary>
        /// 主键
        /// </summary>
        public long ConversationId { get; set; }

        /// <summary>
        /// 会话名称
        /// </summary>
        public string? ConversationName { get; set; }
      
        /// <summary>
        /// 用户id
        /// </summary>
        public long? UserId { get; set; }
    }
}
