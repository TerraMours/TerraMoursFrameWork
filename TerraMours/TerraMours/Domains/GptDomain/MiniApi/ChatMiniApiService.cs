﻿using Azure.AI.OpenAI;
using k8s.KubeConfigModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel.AI.ChatCompletion;
using Serilog;
using StackExchange.Redis;
using System.IO.Pipelines;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading;
using TerraMours.Domains.LoginDomain.Contracts.Common;
using TerraMours_Gpt.Domains.GptDomain.Contracts.Req;
using TerraMours_Gpt.Domains.GptDomain.Contracts.Res;
using TerraMours_Gpt.Domains.GptDomain.IServices;
using TerraMours_Gpt.Domains.LoginDomain.Contracts.Common;
using TerraMours_Gpt.Framework.Infrastructure.Contracts.GptModels;
using TerraMours_Gpt.Framework.Infrastructure.Middlewares;

namespace TerraMours_Gpt.Domains.GptDomain.MiniApi {
    public class ChatMiniApiService : ServiceBase {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IChatService _chatService;
        private readonly Serilog.ILogger _logger;

        public ChatMiniApiService(IHttpContextAccessor httpContextAccessor, IChatService chatService, Serilog.ILogger logger) : base()
        {
            _httpContextAccessor = httpContextAccessor;
            _chatService = chatService;
            App.MapPost("/api/v1/Chat/ChatCompletionStream", ChatCompletionStream);
            App.MapPost("/api/v1/Chat/ChatCompletion", ChatCompletion);
            App.MapPost("/api/v1/Chat/ImportSensitive", ImportSensitive);
            App.MapGet("/api/v1/Chat/AddSensitive", AddSensitive);
            App.MapGet("/api/v1/Chat/ChangeSensitive", ChangeSensitive);
            App.MapGet("/api/v1/Chat/DeleteSensitive", DeleteSensitive);
            App.MapPost("/api/v1/Chat/SensitiveList", SensitiveList);

            App.MapGet("/api/v1/Chat/UpdateKeyOptionsBalance", UpdateKeyOptionsBalance);
            App.MapGet("/api/v1/Chat/AddKeyOptions", AddKeyOptions);
            App.MapGet("/api/v1/Chat/ChangeKeyOptions", ChangeKeyOptions);
            App.MapGet("/api/v1/Chat/DeleteKeyOptions", DeleteKeyOptions);
            App.MapPost("/api/v1/Chat/KeyOptionsList", KeyOptionsList);
            App.MapGet("/api/v1/Chat/CheckBalance", CheckBalance);

            App.MapGet("/api/v1/Chat/AddChatConversation", AddChatConversation);
            App.MapGet("/api/v1/Chat/ChangeChatConversation", ChangeChatConversation);
            App.MapGet("/api/v1/Chat/DeleteChatConversation", DeleteChatConversation);
            App.MapPost("/api/v1/Chat/ChatConversationList", ChatConversationList);

            App.MapPost("/api/v1/Chat/ImportPromptOptionByFile", ImportPromptOptionByFile);
            App.MapPost("/api/v1/Chat/ImportPromptOption", ImportPromptOption);
            App.MapPost("/api/v1/Chat/AddPromptOption", AddPromptOption);
            App.MapPost("/api/v1/Chat/ChangePromptOption", ChangePromptOption);
            App.MapGet("/api/v1/Chat/DeletePromptOption", DeletePromptOption);
            App.MapPost("/api/v1/Chat/PromptOptionList", PromptOptionList);
            App.MapGet("/api/v1/Chat/AllPromptOptionList", AllPromptOptionList);

            App.MapGet("/api/v1/Chat/DeleteChatRecord", DeleteChatRecord);
            App.MapPost("/api/v1/Chat/ChatRecordList", ChatRecordList);
            _logger = logger;
        }
        #region 聊天记录

        /// <summary>
        /// 聊天接口（gpt-4）返回流
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [Authorize]
        [KeyMiddlewareEnabled]
        [Produces("application/octet-stream")]
        public async Task ChatCompletionStream(ChatReq req, CancellationToken cancellationToken = default)
        {
            if (_httpContextAccessor.HttpContext?.Items["key"] != null)
            {
                req.Key = _httpContextAccessor.HttpContext?.Items["key"]?.ToString();
            }
            if (_httpContextAccessor.HttpContext?.Items["baseUrl"] != null)
            {
                req.BaseUrl = _httpContextAccessor.HttpContext?.Items["baseUrl"]?.ToString();
            }
            if (!req.Model.Contains("gpt-4"))
            {
                _logger.Information($"ChatStream开始时间：{DateTime.Now}，key【{req.Key}】");
            }
            var userId = long.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.UserData));
            req.UserId = userId;
            req.IP = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.MapToIPv4().ToString();
            var response = _httpContextAccessor.HttpContext.Response;
            response.ContentType = "application/octet-stream";
            var enumerable = _chatService.ChatCompletionStream(req);
            await foreach (var item in enumerable)
            {
                var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(item, new JsonSerializerOptions()
                {
                    Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
                }) + "\n");
                await response.BodyWriter.WriteAsync(bytes);
            }
            _logger.Information($"ChatStream1结束时间：{DateTime.Now}");
        }
        /// <summary>
        /// 聊天接口（直接返回）
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [Authorize]
        [KeyMiddlewareEnabled]
        public async Task<IResult> ChatCompletion(ChatReq req) {
            if (_httpContextAccessor.HttpContext?.Items["key"] != null) {
                req.Key = _httpContextAccessor.HttpContext?.Items["key"]?.ToString();
            }
            if (_httpContextAccessor.HttpContext?.Items["baseUrl"] != null)
            {
                req.BaseUrl = _httpContextAccessor.HttpContext?.Items["baseUrl"]?.ToString();
            }
            var userId = long.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.UserData));
            req.UserId = userId;
            req.IP = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.MapToIPv4().ToString();
            var res=await _chatService.ChatCompletion(req);
            return Results.Ok(res);
        }
        /// <summary>
        /// 删除聊天记录
        /// </summary>
        /// <param name="recordId">id</param>
        /// <returns></returns>
        [Authorize]
        public async Task<IResult> DeleteChatRecord(long recordId) {
            var userId = long.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.UserData));
            var res = await _chatService.DeleteChatRecord(recordId, userId);
            return Results.Ok(res);
        }
        /// <summary>
        /// 聊天记录列表
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        [Authorize]
        public async Task<IResult> ChatRecordList(ChatRecordReq page) {
            page.UserId = long.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.UserData));
            var res = await _chatService.ChatRecordList(page);
            return Results.Ok(res);
        }
        #endregion
        #region 敏感词

        /// <summary>
        /// 导入敏感词字典
        /// </summary>
        /// <param name="file">敏感词字典文件</param>
        /// <returns></returns>
        [Authorize]
        public async Task<IResult> ImportSensitive(IFormFile file)
        {
            var userId = long.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.UserData));
            var res =await _chatService.ImportSensitive(file, userId);
            return Results.Ok(res);
        }

        /// <summary>
        /// 添加敏感词
        /// </summary>
        /// <param name="word">敏感词</param>
        /// <returns></returns>
        [Authorize]
        public async Task<IResult> AddSensitive(string word)
        {
            var userId = long.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.UserData));
            var res =await _chatService.AddSensitive(word, userId);
            return Results.Ok(res);
        }

        /// <summary>
        /// 修改敏感词
        /// </summary>
        /// <param name="sensitiveId">主键id</param>
        /// <param name="word">敏感词</param>
        /// <returns></returns>
        [Authorize]
        public async Task<IResult> ChangeSensitive(long sensitiveId, string word)
        {
            var userId = long.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.UserData));
            var res =await _chatService.ChangeSensitive(sensitiveId,word, userId);
            return Results.Ok(res);
        }
        /// <summary>
        /// 删除敏感词
        /// </summary>
        /// <param name="sensitiveId"></param>
        /// <returns></returns>
        [Authorize]
        public async Task<IResult> DeleteSensitive(long sensitiveId)
        {
            var userId = long.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.UserData));
            var res =await _chatService.DeleteSensitive(sensitiveId, userId);
            return Results.Ok(res);
        }

        /// <summary>
        /// 敏感词列表
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        [Authorize]
        public async Task<IResult> SensitiveList(PageReq page)
        {
            var res = await _chatService.SensitiveList(page);
            return Results.Ok(res);
        }
        #endregion

        #region Key管理

        /// <summary>
        /// 更新key池的额度使用情况
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public async Task<IResult> UpdateKeyOptionsBalance()
        {
            var userId = long.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.UserData));
            var res = await _chatService.UpdateKeyOptionsBalance(userId);
            return Results.Ok(res);
        }

        /// <summary>
        /// 添加key
        /// </summary>
        /// <param name="apiKey">apiKey</param>
        /// <returns></returns>
        [Authorize]
        public async Task<IResult> AddKeyOptions(string apiKey)
        {
            var userId = long.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.UserData));
            var res = await _chatService.AddKeyOptions(apiKey, userId);
            return Results.Ok(res);
        }

        /// <summary>
        /// 修改key
        /// </summary>
        /// <param name="keyId">id</param>
        /// <param name="apiKey">apiKey</param>
        /// <returns></returns>
        [Authorize]
        public async Task<IResult> ChangeKeyOptions(long keyId, string apiKey)
        {
            var userId = long.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.UserData));
            var res = await _chatService.ChangeKeyOptions(keyId, apiKey, userId);
            return Results.Ok(res);
        }

        /// <summary>
        /// 删除指定key
        /// </summary>
        /// <param name="keyId">id</param>
        /// <returns></returns>
        [Authorize]
        public async Task<IResult> DeleteKeyOptions(long keyId)
        {
            var userId = long.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.UserData));
            var res = await _chatService.DeleteKeyOptions(keyId, userId);
            return Results.Ok(res);
        }
        /// <summary>
        /// key配置列表
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        [Authorize]
        public async Task<IResult> KeyOptionsList(PageReq page)
        {
            var res = await _chatService.KeyOptionsList(page);
            return Results.Ok(res);
        }
        /// <summary>
        /// 检查余额，公共方法
        /// </summary>
        /// <param name="key">openai秘钥</param>
        /// <returns></returns>
        public async Task<IResult> CheckBalance(string key)
        {
            var res = await _chatService.CheckBalance(key);
            return Results.Ok(res);
        }
        #endregion

        #region 会话列表

        /// <summary>
        /// 添加会话
        /// </summary>
        /// <param name="conversationName">会话名称</param>
        /// <returns></returns>
        [Authorize]
        public async Task<IResult> AddChatConversation(string conversationName) {
            var userId = long.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.UserData));
            var res = await _chatService.AddChatConversation(conversationName, userId);
            return Results.Ok(res);
        }

        /// <summary>
        /// 修改会话
        /// </summary>
        /// <param name="conversationId">id</param>
        /// <param name="conversationName">会话名称</param>
        /// <returns></returns>
        [Authorize]
        public async Task<IResult> ChangeChatConversation(long conversationId, string conversationName) {
            var userId = long.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.UserData));
            var res = await _chatService.ChangeChatConversation(conversationId, conversationName, userId);
            return Results.Ok(res);
        }

        /// <summary>
        /// 删除会话
        /// </summary>
        /// <param name="conversationId">id</param>
        /// <returns></returns>
        [Authorize]
        public async Task<IResult> DeleteChatConversation(long conversationId) {
            var userId = long.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.UserData));
            var res = await _chatService.DeleteChatConversation(conversationId, userId);
            return Results.Ok(res);
        }
        /// <summary>
        /// 会话列表
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        [Authorize]
        public async Task<IResult> ChatConversationList(PageReq page) {
            var userId = long.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.UserData));
            var res = await _chatService.ChatConversationList(page,userId);
            return Results.Ok(res);
        }

        #endregion

        #region 系统提示词
        /// <summary>
        /// 导入系统提示词(文件)
        /// </summary>
        /// <param name="file">系统提示词(文件)</param>
        /// <returns></returns>
        [Authorize]
        public async Task<IResult> ImportPromptOptionByFile(IFormFile file)
        {
            var userId = long.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.UserData));
            var res = await _chatService.ImportPromptOptionByFile(file, userId);
            return Results.Ok(res);
        }

        /// <summary>
        /// 导入系统提示词json
        /// </summary>
        /// <param name="prompts">敏感词字典json</param>
        /// <returns></returns>
        [Authorize]
        public async Task<IResult> ImportPromptOption(List<PromptOptionReq> prompts)
        {
            var userId = long.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.UserData));
            var res = await _chatService.ImportPromptOption(prompts, userId);
            return Results.Ok(res);
        }

        /// <summary>
        /// 添加系统提示词
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [Authorize]
        public async Task<IResult> AddPromptOption(PromptDetailReq req)
        {
            req.UserId = long.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.UserData));
            var res = await _chatService.AddPromptOption(req);
            return Results.Ok(res);
        }

        /// <summary>
        /// 修改系统提示词
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [Authorize]
        public async Task<IResult> ChangePromptOption(PromptDetailReq req)
        {
            req.UserId = long.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.UserData));
            var res = await _chatService.ChangePromptOption(req);
            return Results.Ok(res);
        }
        /// <summary>
        /// 删除系统提示词
        /// </summary>
        /// <param name="promptId"></param>
        /// <returns></returns>
        [Authorize]
        public async Task<IResult> DeletePromptOption(long promptId)
        {
            var userId = long.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.UserData));
            var res = await _chatService.DeletePromptOption(promptId, userId);
            return Results.Ok(res);
        }

        /// <summary>
        /// 系统提示词列表
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        [Authorize]
        public async Task<IResult> PromptOptionList(PageReq page)
        {
            var res = await _chatService.PromptOptionList(page);
            return Results.Ok(res);
        }

        /// <summary>
        /// 获取全部系统提示词列表
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        [Authorize]
        public async Task<IResult> AllPromptOptionList() {
            var res = await _chatService.AllPromptOptionList();
            return Results.Ok(res);
        }
        
        #endregion
    }
}
