﻿using TerraMours.Domains.LoginDomain.Contracts.Common;
using TerraMours.Domains.LoginDomain.Contracts.Req;
using TerraMours_Gpt.Domains.GptDomain.Contracts.Req;
using TerraMours_Gpt.Domains.GptDomain.Contracts.Res;
using TerraMours_Gpt.Domains.LoginDomain.Contracts.Common;
using TerraMours_Gpt.Framework.Infrastructure.Contracts.GptModels;

namespace TerraMours_Gpt.Domains.GptDomain.IServices {
    public interface IChatService {
        /// <summary>
        /// 聊天接口
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        IAsyncEnumerable<string> ChatProcessStream(ChatReq req);

        #region 敏感词
        Task<ApiResponse<bool>> ImportSensitive(IFormFile file,long? userId);
        Task<ApiResponse<bool>> AddSensitive(string word, long? userId);
        Task<ApiResponse<bool>> ChangeSensitive(long sensitiveId,string word, long? userId);
        Task<ApiResponse<bool>> DeleteSensitive(long sensitiveId, long? userId);
        Task<ApiResponse<PagedRes<SensitiveRes>>> SensitiveList(PageReq page);
        #endregion

        #region Key管理
        Task<ApiResponse<bool>> UpdateKeyOptionsBalance(long? userId);
        Task<ApiResponse<bool>> AddKeyOptions(string apiKey, long? userId);
        Task<ApiResponse<bool>> ChangeKeyOptions(long keyId, string apiKey, long? userId);
        Task<ApiResponse<bool>> DeleteKeyOptions(long keyId, long? userId);
        Task<ApiResponse<PagedRes<KeyOptionRes>>> KeyOptionsList(PageReq page);
        Task<ApiResponse<CheckBalanceRes>> CheckBalance(string key);
        #endregion

        #region 会话列表
        Task<ApiResponse<bool>> AddChatConversation(string conversationName, long? userId);
        Task<ApiResponse<bool>> ChangeChatConversation(long conversationId, string conversationName, long? userId);
        Task<ApiResponse<bool>> DeleteChatConversation(long conversationId, long? userId);
        Task<ApiResponse<PagedRes<ChatConversationRes>>> ChatConversationList(PageReq page, long? userId);
        #endregion
    }
}
