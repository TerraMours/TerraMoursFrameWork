using TerraMours_Gpt.Domains.GptDomain.Contracts.Req;
using TerraMours_Gpt.Domains.GptDomain.Contracts.Res;
using TerraMours_Gpt.Domains.GptDomain.IServices;

namespace TerraMours_Gpt.Domains.GptDomain.Services {
    public class ChatService : IChatService {
        public async IAsyncEnumerable<ChatRes> ChatProcessStream(ChatReq req) {
            throw new NotImplementedException();
        }
    }
}
