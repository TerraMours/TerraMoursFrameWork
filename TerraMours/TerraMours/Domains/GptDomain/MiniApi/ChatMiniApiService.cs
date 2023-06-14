using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.IO.Pipelines;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using TerraMours_Gpt.Domains.GptDomain.Contracts.Req;
using TerraMours_Gpt.Domains.GptDomain.IServices;

namespace TerraMours_Gpt.Domains.GptDomain.MiniApi {
    public class ChatMiniApiService : ServiceBase {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IChatService _chatService;

        public ChatMiniApiService(IHttpContextAccessor httpContextAccessor, IChatService chatService) : base() {
            _httpContextAccessor = httpContextAccessor;
            _chatService = chatService;
            App.MapPost("/api/v1/Chat/ChatStream", ChatStream);
        }
        [Authorize]
        [Produces("application/octet-stream")]
        public async Task<IResult> ChatStream(ChatReq req) {
            if (_httpContextAccessor.HttpContext?.Items["key"] !=null) {
                req.Key = _httpContextAccessor.HttpContext?.Items["key"]?.ToString();
            }
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
    }
}
