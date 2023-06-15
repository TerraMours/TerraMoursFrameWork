using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using StackExchange.Redis;
using System.IO.Pipelines;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using TerraMours.Domains.LoginDomain.Contracts.Common;
using TerraMours_Gpt.Domains.GptDomain.Contracts.Req;
using TerraMours_Gpt.Domains.GptDomain.Contracts.Res;
using TerraMours_Gpt.Domains.GptDomain.IServices;
using TerraMours_Gpt.Framework.Infrastructure.Contracts.GptModels;

namespace TerraMours_Gpt.Domains.GptDomain.MiniApi {
    public class ChatMiniApiService : ServiceBase {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IChatService _chatService;

        public ChatMiniApiService(IHttpContextAccessor httpContextAccessor, IChatService chatService) : base() {
            _httpContextAccessor = httpContextAccessor;
            _chatService = chatService;
            App.MapPost("/api/v1/Chat/ChatStream", ChatStream);

            App.MapPost("/api/v1/Chat/ImportSensitive", ImportSensitive);
            App.MapGet("/api/v1/Chat/AddSensitive", AddSensitive);
            App.MapGet("/api/v1/Chat/ChangeSensitive", ChangeSensitive);
            App.MapGet("/api/v1/Chat/DeleteSensitive", DeleteSensitive);
        }
        [Authorize]
        [Produces("application/octet-stream")]
        public async Task<IResult> ChatStream(ChatReq req) {
            if (_httpContextAccessor.HttpContext?.Items["key"] !=null) {
                req.Key = _httpContextAccessor.HttpContext?.Items["key"]?.ToString();
            }
            var userId = long.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.UserData));
            req.UserId=userId;
            req.IP = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.MapToIPv4().ToString();
            var enumerable = _chatService.ChatProcessStream(req);
            var pipe = new Pipe();
            _ = Task.Run(async () => {
                try {
                    await foreach (var item in enumerable.WithCancellation(default)) {
                        var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(item, new JsonSerializerOptions() {
                            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
                        }) + "\n");
                        await pipe.Writer.WriteAsync(bytes, default);
                    }
                }
                finally {
                    pipe.Writer.Complete();
                }
            }, default);
            return Results.Ok(pipe.Reader.AsStream());
        }
        #region 敏感词
        [Authorize]
        public async Task<IResult> ImportSensitive(IFormFile file)
        {
            var userId = long.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.UserData));
            var res =await _chatService.ImportSensitive(file, userId);
            return Results.Ok(res);
        }
        [Authorize]
        public async Task<IResult> AddSensitive(string word)
        {
            var userId = long.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.UserData));
            var res =await _chatService.AddSensitive(word, userId);
            return Results.Ok(res);
        }
        [Authorize]
        public async Task<IResult> ChangeSensitive(long sensitiveId, string word)
        {
            var userId = long.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.UserData));
            var res =await _chatService.ChangeSensitive(sensitiveId,word, userId);
            return Results.Ok(res);
        }
        [Authorize]
        public async Task<IResult> DeleteSensitive(long sensitiveId)
        {
            var userId = long.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.UserData));
            var res =await _chatService.DeleteSensitive(sensitiveId, userId);
            return Results.Ok(res);
        }
        #endregion

        #region Key管理
        [Authorize]
        public async Task<IResult> UpdateKeyOptionsBalance()
        {
            var userId = long.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.UserData));
            var res = await _chatService.UpdateKeyOptionsBalance(userId);
            return Results.Ok(res);
        }
        [Authorize]
        public async Task<IResult> AddKeyOptions(string apiKey)
        {
            var userId = long.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.UserData));
            var res = await _chatService.AddKeyOptions(apiKey, userId);
            return Results.Ok(res);
        }
        [Authorize]
        public async Task<IResult> ChangeKeyOptions(long keyId, string apiKey)
        {
            var userId = long.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.UserData));
            var res = await _chatService.ChangeKeyOptions(keyId, apiKey, userId);
            return Results.Ok(res);
        }
        [Authorize]
        public async Task<IResult> DeleteKeyOptions(long keyId)
        {
            var userId = long.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.UserData));
            var res = await _chatService.DeleteKeyOptions(keyId, userId);
            return Results.Ok(res);
        }
        /// <summary>
        /// 检查余额，公共方法
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<IResult> CheckBalance(string key)
        {
            var res = await _chatService.CheckBalance(key);
            return Results.Ok(res);
        }
        #endregion
    }
}
