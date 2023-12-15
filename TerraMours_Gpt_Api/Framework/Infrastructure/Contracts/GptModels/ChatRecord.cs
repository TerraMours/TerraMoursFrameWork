﻿using Microsoft.EntityFrameworkCore.Metadata.Builders;
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
        /// <summary>
        /// 使用者IP
        /// </summary>
        public string? IP { get; set; }

        /// <summary>
        /// 角色
        /// </summary>
        public string? Role { get; set; }

        /// <summary>
        /// 信息
        /// </summary>
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

        /// <summary>
        /// 是否可用 重要
        /// </summary>
        public bool Enable { get; set; }

        /// <summary>
        /// 提问词Tokens
        /// </summary>
        public int? PromptTokens { get; set; }
        /// <summary>
        /// 回答Tokens
        /// </summary>

        public int? CompletionTokens { get; set; }
        /// <summary>
        /// 总Tokens
        /// </summary>

        public int? TotalTokens { get; set; }

        //public ChatConversation? ChatConversation { get; set; }

        public ChatRecord()
        {

        }

        public ChatRecord Delete(long? userId) {
            this.Enable = false;
            //EntityBase
            this.ModifyDate = DateTime.Now;
            return this;
        }
    }
   
}
