using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AI.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI.ChatCompletion;
using TerraMours.Framework.Infrastructure.Contracts.Commons;
using TerraMours.Framework.Infrastructure.Contracts.SystemModels;
using TerraMours.Framework.Infrastructure.EFCore;
using TerraMours.Framework.Infrastructure.Redis;
using TerraMours_Gpt.Domains.GptDomain.Contracts.Enum;
using TerraMours_Gpt.Domains.GptDomain.Contracts.Req;
using TerraMours_Gpt.Domains.GptDomain.Contracts.Res;
using TerraMours_Gpt.Domains.GptDomain.IServices;
using TerraMours_Gpt.Framework.Infrastructure.Contracts.Commons;
using TerraMours_Gpt.Framework.Infrastructure.Contracts.GptModels;

namespace TerraMours_Gpt.Domains.GptDomain.Services {
    public class ChatService : IChatService {
        private readonly FrameworkDbContext _dbContext;
        private readonly IOptionsSnapshot<GptOptions> _options;
        private readonly IMapper _mapper;
        private readonly IDistributedCacheHelper _helper;

        public ChatService(FrameworkDbContext dbContext, IOptionsSnapshot<GptOptions> options, IMapper mapper, IDistributedCacheHelper helper)
        {
            _dbContext = dbContext;
            _options = options;
            _mapper = mapper;
            _helper = helper;
        }

        public async IAsyncEnumerable<ChatRes> ChatProcessStream(ChatReq req) {
            var user = await getSysUser(req.UserId);

            //敏感词检测
            if (!Sensitive(req))
            {
                yield return new ChatRes("触发了敏感词");
            }
            else if(!await CodeCanAsk(user))
            {
                if (user.VipLevel>0)
                {
                    yield return new ChatRes("请勿恶意使用");
                }
                else
                    yield return new ChatRes($"已超过体验账号每天最大提问次数：{_options.Value.OpenAIOptions.OpenAI.MaxQuestions}次");
            }
            else
            {
                IKernel kernel= Kernel.Builder.Build();
                kernel.Config.AddOpenAIChatCompletionService("chat", req.Options.Model ?? _options.Value.OpenAIOptions.OpenAI.ChatModel,
                req.Key);
                var chatCompletion = kernel.GetService<IChatCompletion>();
                var options = new ChatRequestSettings()
                {
                    MaxTokens = _options.Value.OpenAIOptions.OpenAI.MaxTokens,
                    Temperature = _options.Value.OpenAIOptions.OpenAI.Temperature,
                    FrequencyPenalty = _options.Value.OpenAIOptions.OpenAI.FrequencyPenalty,
                    PresencePenalty = _options.Value.OpenAIOptions.OpenAI.PresencePenalty,
                    TopP = _options.Value.OpenAIOptions.OpenAI.TopP
                };
                var chatHistory = new OpenAIChatHistory();
                chatHistory.AddUserMessage(req.Prompt);
                var chatMeg = chatCompletion?.GenerateMessageStreamAsync(chatHistory, options);
                //接口返回的完整内容
                string totalMsg = "";
                await foreach(var msg in chatMeg)
                {
                    if(msg != null)
                    {
                        totalMsg += msg;
                    }
                    yield return new ChatRes(totalMsg);
                }
                   
            }
            throw new NotImplementedException();
        }

        #region 私有方法
        /// <summary>
        /// 敏感词检测
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        private bool Sensitive(ChatReq req)
        {
            bool res = true;
            if (_dbContext.Sensitives.Count(m => req.Prompt.Contains(m.Word)) > 0)
            {
                using (var context = _dbContext)
                {
                    //保存提问信息
                    var problem = new ChatRecord();
                    problem.IP = req.IP;
                    problem.Message = "敏感提问：" + req.Prompt;
                    problem.Role = "user";
                    problem.CreateDate = DateTime.Now;
                    problem.UserId = req.UserId;
                    problem.ModelType=req.Options.ModelType ?? ((int)ModelTypeEnum.ChatGpt);
                    problem.Model =req.Options.Model ?? _options.Value.OpenAIOptions.OpenAI.ChatModel;
                    context.Add(problem);
                    context.SaveChangesAsync();
                }
                res = false;
            }
            return res;
        }
        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        private async Task<SysUser?> getSysUser(long userId)
        {
            return await _helper.GetOrCreateAsync($"SysUser_{userId}", async options => { return await _dbContext.SysUsers.FirstOrDefaultAsync(m => m.UserId == userId); });
        }
        /// <summary>
        /// 获取当前用户当日提问次数
        /// </summary>
        /// <returns></returns>
        private async Task<int> TodayVisits(long userId)
        {
            return await _helper.GetOrCreateAsync($"TodayVisits_{userId}", async options => { return await _dbContext.ChatRecords.CountAsync(m => m.UserId == userId && m.CreateDate.Date == DateTime.Today.Date && m.Role == "user"); });
        }
        /// <summary>
        /// 验证码能否提问
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        private async Task<bool> CodeCanAsk(SysUser user)
        {
            bool res = true;
            int max = user.VipLevel>0 ? 999 : (_options.Value.OpenAIOptions.OpenAI.MaxQuestions);
            if (await TodayVisits(user.UserId) > max)
            {
                res = false;
            }
            return res;
        }
        #endregion
    }
}
