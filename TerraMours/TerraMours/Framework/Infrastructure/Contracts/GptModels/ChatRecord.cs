using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace TerraMours_Gpt.Framework.Infrastructure.Contracts.GptModels
{
    /// <summary>
    /// 聊天记录
    /// </summary>
    public class ChatRecord
    {
        /// <summary>
        /// 主键
        /// </summary>
        public long ChatRecordId { get; set; }

        /// <summary>
        /// 模型类型
        /// </summary>
        public int? ModelType { get; set; }
        /// <summary>
        /// 聊天模型
        /// </summary>
        public string? Model { get; set; }
        /// <summary>
        /// 会话id
        /// </summary>
        public long? ConversationId { get; set; }
        public string? IP { get; set; }
        public string? Role { get; set; }
        public string? Message { get; set; }
        /// <summary>
        /// 用户id
        /// </summary>
        public long? UserId { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateDate { get; set; }
        /// <summary>
        /// 修改时间
        /// </summary>
        public DateTime? ModifyDate { get; set; }

        public ChatConversation? ChatConversation { get; set; }
    }
   
}
