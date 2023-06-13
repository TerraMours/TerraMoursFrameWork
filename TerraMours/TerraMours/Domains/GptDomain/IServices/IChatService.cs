using TerraMours_Gpt.Domains.GptDomain.Contracts.Req;
using TerraMours_Gpt.Domains.GptDomain.Contracts.Res;

namespace TerraMours_Gpt.Domains.GptDomain.IServices {
    public interface IChatService {
        /// <summary>
        /// 聊天接口
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        IAsyncEnumerable<ChatRes> ChatProcessStream(ChatReq req);
    }
}
