﻿namespace TerraMours_Gpt.Domains.GptDomain.Contracts.Res {
    public class ChatConversationRes {
        /// <summary>
        /// 主键
        /// </summary>
        public long ConversationId { get; set; }

        /// <summary>
        /// 会话名称
        /// </summary>
        public string? ConversationName { get; set; }
    }
}
