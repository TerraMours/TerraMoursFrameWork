using AutoMapper;
using k8s.KubeConfigModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AI.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI.ChatCompletion;
using System.Linq;
using System.Text;
using System.Text.Json;
using TerraMours.Domains.LoginDomain.Contracts.Common;
using TerraMours.Framework.Infrastructure.Contracts.Commons;
using TerraMours.Framework.Infrastructure.Contracts.SystemModels;
using TerraMours.Framework.Infrastructure.EFCore;
using TerraMours.Framework.Infrastructure.Redis;
using TerraMours_Gpt.Domains.GptDomain.Contracts.Enum;
using TerraMours_Gpt.Domains.GptDomain.Contracts.Req;
using TerraMours_Gpt.Domains.GptDomain.Contracts.Res;
using TerraMours_Gpt.Domains.GptDomain.IServices;
using TerraMours_Gpt.Domains.LoginDomain.Contracts.Common;
using TerraMours_Gpt.Framework.Infrastructure.Contracts.Commons;
using TerraMours_Gpt.Framework.Infrastructure.Contracts.GptModels;

namespace TerraMours_Gpt.Domains.GptDomain.Services {
    public class ChatService : IChatService {
        private readonly FrameworkDbContext _dbContext;
        private readonly IOptionsSnapshot<GptOptions> _options;
        private readonly IMapper _mapper;
        private readonly IDistributedCacheHelper _helper;
        private readonly Serilog.ILogger _logger;
        private readonly HttpClient _httpClient;

        public ChatService(FrameworkDbContext dbContext, IOptionsSnapshot<GptOptions> options, IMapper mapper, IDistributedCacheHelper helper, Serilog.ILogger logger, HttpClient httpClient)
        {
            _dbContext = dbContext;
            _options = options;
            _mapper = mapper;
            _helper = helper;
            _logger = logger;
            _httpClient = httpClient;
        }
        #region 聊天
        /// <summary>
        /// 聊天接口
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async IAsyncEnumerable<ChatRes> ChatProcessStream(ChatReq req) {
            var user = await getSysUser(req.UserId);

            //敏感词检测
            if (!Sensitive(req))
            {
                yield return new ChatRes("触发了敏感词");
            }
            else if (!await CodeCanAsk(user))
            {
                if (user.VipLevel > 0)
                {
                    yield return new ChatRes("请勿恶意使用");
                }
                else
                    yield return new ChatRes($"已超过体验账号每天最大提问次数：{_options.Value.OpenAIOptions.OpenAI.MaxQuestions}次");
            }
            else
            {
                IKernel kernel = Kernel.Builder.Build();
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
                await foreach (var msg in chatMeg)
                {
                    if (msg != null)
                    {
                        totalMsg += msg;
                    }
                    yield return new ChatRes(totalMsg);
                }

            }
            throw new NotImplementedException();
        }
        #endregion

        #region 敏感词
        /// <summary>
        /// 导入敏感词
        /// </summary>
        /// <param name="file"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<ApiResponse<bool>> ImportSensitive(IFormFile file, long? userId)
        {
            try
            {
                using (var context = _dbContext)
                {
                    // 打开文本文件
                    using (StreamReader sr = new StreamReader(file.OpenReadStream()))
                    {
                        string line;
                        // 逐行读取文本内容
                        while ((line = sr.ReadLine()) != null)
                        {
                            // 读取文件内容并Base64解码

                            var decodedContent = Encoding.UTF8.GetString(Convert.FromBase64String(line));
                            await context.Sensitives.AddAsync(new Sensitive(decodedContent, userId));
                        }
                    }
                    await context.SaveChangesAsync();
                }
                return ApiResponse<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error inserting data into Sensitive table");
                return ApiResponse<bool>.Fail(ex.ToString());
            }
        }

        /// <summary>
        /// 添加敏感词
        /// </summary>
        /// <param name="word"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<ApiResponse<bool>> AddSensitive(string word, long? userId)
        {
            await _dbContext.Sensitives.AddAsync(new Sensitive(word, userId));
            await _dbContext.SaveChangesAsync();
            return ApiResponse<bool>.Success(true);
        }
        /// <summary>
        /// 修改敏感词
        /// </summary>
        /// <param name="sensitiveId"></param>
        /// <param name="word"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<ApiResponse<bool>> ChangeSensitive(long sensitiveId, string word, long? userId)
        {
            var sensitive = await _dbContext.Sensitives.FirstOrDefaultAsync(m => m.SensitiveId == sensitiveId && m.Enable == true);
            sensitive?.Change(word, userId);
            await _dbContext.SaveChangesAsync();
            return ApiResponse<bool>.Success(true);
        }
        /// <summary>
        /// 删除敏感词
        /// </summary>
        /// <param name="verificationId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<ApiResponse<bool>> DeleteSensitive(long sensitiveId, long? userId)
        {
            var sensitive = await _dbContext.Sensitives.FirstOrDefaultAsync(m => m.SensitiveId == sensitiveId && m.Enable == true);
            sensitive?.Delete(userId);
            await _dbContext.SaveChangesAsync();
            return ApiResponse<bool>.Success(true);
        }
        /// <summary>
        /// 敏感词列表
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<ApiResponse<PagedRes<SensitiveRes>>> SensitiveList(PageReq page)
        {
            var query = _dbContext.Sensitives.Where(m=> string.IsNullOrEmpty(page.QueryString) || m.Word.Contains(page.QueryString));
            var total = await query.CountAsync();
            var item = await query.Skip((page.PageIndex - 1) * page.PageSize).Take(page.PageSize).ToListAsync();
            var res = _mapper.Map<IEnumerable<SensitiveRes>>(item);
            return ApiResponse<PagedRes<SensitiveRes>>.Success(new PagedRes<SensitiveRes>(res, total, page.PageIndex, page.PageSize));
        }
        #endregion

        #region Key管理
        /// <summary>
        /// 更新key使用情况
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<ApiResponse<bool>> UpdateKeyOptionsBalance(long? userId)
        {
            var keys = _dbContext.KeyOptions.Where(m => m.Enable == true);
            foreach (var key in keys)
            {
                var balance = await getBalanceByKey(key.ApiKey);
                key.UpdateUsed(balance.Used, balance.UnUsed, balance.Total, userId);
            }
            await _dbContext.SaveChangesAsync();
            return ApiResponse<bool>.Success(true);
        }
        public async Task<ApiResponse<bool>> AddKeyOptions(string apiKey, long? userId)
        {
            await _dbContext.KeyOptions.AddAsync(new KeyOptions(apiKey, userId));
            await _dbContext.SaveChangesAsync();
            return ApiResponse<bool>.Success(true);
        }

        public async Task<ApiResponse<bool>> ChangeKeyOptions(long keyId, string apiKey, long? userId)
        {
            var sensitive = await _dbContext.KeyOptions.FirstOrDefaultAsync(m => m.KeyId == keyId && m.Enable == true);
            sensitive?.Change(apiKey, userId);
            await _dbContext.SaveChangesAsync();
            return ApiResponse<bool>.Success(true);
        }
        /// <summary>
        /// 删除key，真删除
        /// </summary>
        /// <param name="keyId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<ApiResponse<bool>> DeleteKeyOptions(long keyId, long? userId)
        {
            var sensitive = await _dbContext.KeyOptions.FirstOrDefaultAsync(m => m.KeyId == keyId);
            _dbContext.KeyOptions.Remove(sensitive);
            await _dbContext.SaveChangesAsync();
            return ApiResponse<bool>.Success(true);
        }
        /// <summary>
        /// key列表
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<ApiResponse<PagedRes<KeyOptionRes>>> KeyOptionsList(PageReq page)
        {
            var query = _dbContext.KeyOptions.Where(m => string.IsNullOrEmpty(page.QueryString) || m.ApiKey.Contains(page.QueryString));
            var total=await query.CountAsync();
            var item=await query.Skip((page.PageIndex-1)* page.PageSize).Take(page.PageSize).ToListAsync();
            var res=_mapper.Map<IEnumerable< KeyOptionRes>>(item);
            return ApiResponse<PagedRes<KeyOptionRes>>.Success(new PagedRes<KeyOptionRes>(res,total,page.PageIndex,page.PageSize));
        }
        /// <summary>
        /// 余额查询
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<ApiResponse<CheckBalanceRes>> CheckBalance(string key)
        {
            // 添加token
            _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + key);
            //1.调用订阅接口
            var message = await _httpClient.GetAsync("https://api.openai.com/v1/dashboard/billing/subscription");
            if (!message.IsSuccessStatusCode)
            {
                return ApiResponse<CheckBalanceRes>.Fail($"报错了：{await message.Content.ReadAsStringAsync()}");
            }
            string subscriptionsonResponse = await message.Content.ReadAsStringAsync();
            if (subscriptionsonResponse.Contains("error"))
            {
                // 请求失败
                ErrorRes errorResponse = JsonSerializer.Deserialize<ErrorRes>(subscriptionsonResponse);
                string errorMessage = errorResponse.Error.Message;
                return ApiResponse<CheckBalanceRes>.Fail($"报错了：{errorMessage}");
            }
            BillingSubscriptionRes billing = await message.Content.ReadFromJsonAsync<BillingSubscriptionRes>();

            var endTime = DateTimeOffset.FromUnixTimeSeconds(billing.AccessUntil).LocalDateTime;
            var showMsg = "";
            if (DateTime.Now > endTime)
            {
                showMsg = "您的账户额度已过期, 请登录OpenAI进行查看。";
                return ApiResponse<CheckBalanceRes>.Fail(showMsg);
            }
            //2.调用查使用量接口
            //接口只支持查一百天
            DateTime now = DateTime.Now;
            DateTime startDate = now.AddDays(-90);
            DateTime endDate = now.AddDays(1);

            //总额度
            var totalAmount = billing.HardLimitUsd;
            var isSubsrcibed = billing.HasPaymentMethod;
            //计算余额等信息
            //计算已使用量
            // 如果用户绑卡，额度每月会刷新
            if (isSubsrcibed)
            {
                startDate = new DateTime(now.Year, now.Month, 1); // 本月第一天的日期
            }
            UsageRes usage = await GetUsage(startDate, endDate);
            //总使用量，绑卡用户为当月使用量
            var totalUsage = usage.TotalUsage / 100;
            // 计算剩余额度
            var remaining = totalAmount - totalUsage;

            //参考https://github.com/herobrine19/openai-billing/blob/main/index.html
            _logger.Information($"余额查询接口调用[{DateTime.Now}]：key:{key},总额度{totalAmount}剩余{remaining}");
            return ApiResponse<CheckBalanceRes>.Success(new CheckBalanceRes(key, endTime, totalUsage, remaining, totalAmount));
        }
        #endregion
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
        /// <summary>
        /// 调用openai查询使用量接口
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        private async Task<UsageRes> GetUsage(DateTime startDate, DateTime endDate)
        {
            var usageUrl = $"https://api.openai.com/v1/dashboard/billing/usage?start_date={startDate.ToString("yyyy-MM-dd")}&end_date={endDate.ToString("yyyy-MM-dd")}";
            var usageMessage = await _httpClient.GetAsync(usageUrl);
            string usageResponse = await usageMessage.Content.ReadAsStringAsync();
            return await usageMessage.Content.ReadFromJsonAsync<UsageRes>();
        }
        private async Task<CheckBalanceRes> getBalanceByKey(string key)
        {
            // 添加token
            //HttpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + key);
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", key);
            var billing = await GetSubscription();
            var endTime = DateTimeOffset.FromUnixTimeSeconds(billing.AccessUntil).LocalDateTime;
            //接口只支持查一百天
            DateTime now = DateTime.Now;
            DateTime startDate = now.AddDays(-90);
            DateTime endDate = now.AddDays(1);

            //总额度
            var totalAmount = billing.HardLimitUsd;
            var isSubsrcibed = billing.HasPaymentMethod;
            //计算余额等信息
            //计算已使用量
            // 如果用户绑卡，额度每月会刷新
            if (isSubsrcibed)
            {
                startDate = new DateTime(now.Year, now.Month, 1); // 本月第一天的日期
            }
            UsageRes usage = await GetUsage(startDate, endDate);
            //总使用量，绑卡用户为当月使用量
            var totalUsage = usage.TotalUsage / 100;
            // 计算剩余额度
            var remaining = totalAmount - totalUsage;
            return new CheckBalanceRes(key, endTime, totalUsage, remaining, totalAmount);
        }
        /// <summary>
        /// 调用openai查询订阅接口
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        private async Task<BillingSubscriptionRes> GetSubscription()
        {
            var message = await _httpClient.GetAsync("https://api.openai.com/v1/dashboard/billing/subscription");
            string subscriptionsonResponse = await message.Content.ReadAsStringAsync();
            return await message.Content.ReadFromJsonAsync<BillingSubscriptionRes>();
        }







        #endregion
    }
}
