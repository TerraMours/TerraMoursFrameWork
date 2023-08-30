using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AI.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI.ChatCompletion;
using OpenAI.Managers;
using OpenAI.ObjectModels.RequestModels;
using Serilog;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using TerraMours.Domains.LoginDomain.Contracts.Common;
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

namespace TerraMours_Gpt.Domains.GptDomain.Services
{
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
        public async IAsyncEnumerable<string> ChatProcessStream(ChatReq req) {
            //创建会话
            if (req.ConversationId ==null || req.ConversationId == 0) {
                var conversation = await _dbContext.ChatConversations.AddAsync(new ChatConversation(req.Prompt.Length < 5 ? req.Prompt : $"{req.Prompt.Substring(0, 10)}...", req.UserId));
                await _dbContext.SaveChangesAsync();
                req.ConversationId= conversation.Entity.ConversationId;
            }

            var user = await getSysUser(req.UserId);

            //敏感词检测
            if (!await Sensitive(req))
            {
                yield return "触发了敏感词";
            }
            else if (!await CodeCanAsk(user)) {
                if (user.VipLevel > 0) {
                    yield return "请勿恶意使用";
                }
                else
                    yield return $"已超过体验账号每天最大提问次数：{_options.Value.OpenAIOptions.OpenAI.MaxQuestions}次";
            }
            else
            {
                _logger.Information($"chat配置：{_options.Value.OpenAIOptions.OpenAI.ChatModel},key:{req.Key}");
                await _dbContext.ChatRecords.AddAsync(new ChatRecord() { Role = "User", Message = req.Prompt, Model = req.Model, ModelType = req.ModelType, ConversationId = req.ConversationId, CreateDate = DateTime.Now, UserId = req.UserId });
                IKernel kernel = Kernel.Builder.Build();
                kernel.Config.AddOpenAIChatCompletionService(req.Model ?? _options.Value.OpenAIOptions.OpenAI.ChatModel,
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
                //接口返回的完整内容
                string totalMsg = "";
                var messageStream = chatCompletion.GenerateMessageStreamAsync(chatHistory, options);
                if (messageStream == null) {
                    yield return "服务器发生了未知的错误";
                }
                await foreach (string msg in messageStream) {
                    if (msg != null) {
                        totalMsg += msg;
                        yield return totalMsg;
                    }
                }
                //记录
                await _dbContext.ChatRecords.AddAsync(new ChatRecord() { Role = "Assistant", Message = totalMsg, Model = req.Model, ModelType = req.ModelType, ConversationId = req.ConversationId, CreateDate = DateTime.Now, UserId = req.UserId });
                await _dbContext.SaveChangesAsync();
            }
        }

        public async IAsyncEnumerable<ApiResponse<ChatRes>> ChatStream(ChatReq req)
        {
            //创建会话
            if (req.ConversationId == null || req.ConversationId == 0)
            {
                var conversation = await _dbContext.ChatConversations.AddAsync(new ChatConversation(req.Prompt.Length < 5 ? req.Prompt : $"{req.Prompt.Substring(0, 5)}...", req.UserId));
                await _dbContext.SaveChangesAsync();
                req.ConversationId = conversation.Entity.ConversationId;
            }

            var user = await getSysUser(req.UserId);
            //敏感词检测
            if (!await Sensitive(req))
            {
                yield return ApiResponse<ChatRes>.Fail("触发了敏感词");
                yield break;
            }
            else if (!await CodeCanAsk(user))
            {
                if (user.VipLevel > 0)
                {
                    yield return ApiResponse<ChatRes>.Fail("请勿恶意使用");
                }
                else
                {
                    yield return ApiResponse<ChatRes>.Fail($"已超过体验账号每天最大提问次数：{_options.Value.OpenAIOptions.OpenAI.MaxQuestions}次");
                }
                yield break;
            }
            else
            {
                _logger.Information($"chat配置：{_options.Value.OpenAIOptions.OpenAI.ChatModel},key:{req.Key}。时间{DateTime.Now}");
                await _dbContext.ChatRecords.AddAsync(new ChatRecord() { Role = "User", Message = req.Prompt, Model = req.Model, ModelType = req.ModelType, ConversationId = req.ConversationId, CreateDate = DateTime.Now, UserId = req.UserId });
                IKernel kernel = Kernel.Builder.Build();
                kernel.Config.AddOpenAIChatCompletionService(req.Model ?? _options.Value.OpenAIOptions.OpenAI.ChatModel,
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
                //接口返回的完整内容
                string totalMsg = "";
                var chatRes= new ChatRes() { Role = "Assistant", Message = totalMsg, Model = req.Model, ModelType = req.ModelType, ConversationId = req.ConversationId, CreateDate = DateTime.Now, UserId = req.UserId };
                var messageStream = chatCompletion.GenerateMessageStreamAsync(chatHistory, options);
                if (messageStream == null)
                {
                    yield return ApiResponse<ChatRes>.Fail("服务器发生了未知的错误");
                }
                await foreach (string msg in messageStream)
                {
                    if (msg != null)
                    {
                        totalMsg += msg;
                        chatRes.Message = totalMsg;
                        yield return ApiResponse<ChatRes>.Success(chatRes);
                    }
                }
                //记录
                var chatRecord=_mapper.Map<ChatRecord>(chatRes);
                await _dbContext.ChatRecords.AddAsync(chatRecord);
                await _dbContext.SaveChangesAsync();
            }
        }
        /// <summary>
        /// 聊天接口（gpt-4）
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async IAsyncEnumerable<ApiResponse<ChatRes>> ChatCompletionStream(ChatReq req)
        {
            //创建会话
            if (req.ConversationId == null || req.ConversationId == 0)
            {
                var conversation = await _dbContext.ChatConversations.AddAsync(new ChatConversation(req.Prompt.Length < 5 ? req.Prompt : $"{req.Prompt.Substring(0, 5)}...", req.UserId));
                _dbContext.SaveChanges();
                req.ConversationId = conversation.Entity.ConversationId;
            }

            var user = await getSysUser(req.UserId);
            if (user ==null)
            {
                yield return ApiResponse<ChatRes>.Fail("当前用户不存在");
                yield break;
            }
            //敏感词检测
            if (!await Sensitive(req))
            {
                yield return ApiResponse<ChatRes>.Fail("触发了敏感词");
                yield break;
            }
            
            //上下文
            List<ChatMessage> messegs =await BuildMsgList(req);
            decimal takesPrice = GetAskPrice(messegs, req.Model ?? _options.Value.OpenAIOptions.OpenAI.ChatModel);
            //判断余额，gpt4时需要余额五元以上
            if (user.Balance == null || user.Balance <=( req.Model== "gpt-4"? (takesPrice+5):takesPrice))
            {
                yield return ApiResponse<ChatRes>.Fail($"账号余额不足，请充值");
                yield break;
            }
            
            int maxtoken;
            switch (req.Model) {
                case "gpt-4":
                    maxtoken = (int)((req.MaxTokens != null && req.MaxTokens < 4000) ? req.MaxTokens : 4000);
                    break;
                case "gpt-3.5-turbo-16k":
                    maxtoken = (int)((req.MaxTokens != null && req.MaxTokens < 4000) ? req.MaxTokens : 4000);
                    break;
                default:
                    maxtoken = (int)((req.MaxTokens != null && req.MaxTokens < _options.Value.OpenAIOptions.OpenAI.MaxTokens) ? req.MaxTokens : _options.Value.OpenAIOptions.OpenAI.MaxTokens);
                    break;
            }
            string apikey;
            if (req.Model == "gpt-4")
                apikey = _options.Value.OpenAIOptions.OpenAI.Gpt4Key;
            else 
                apikey=req.Key;

            var openAiOpetions = new OpenAI.OpenAiOptions()
            {
                ApiKey = req.Key,
                BaseDomain= _options.Value.OpenAIOptions.OpenAI.BaseUrl
            };
            //gpt-4走的第三方
            if (req.Model == "gpt-4")
            {
                openAiOpetions.ApiKey= _options.Value.OpenAIOptions.OpenAI.Gpt4Key;
                openAiOpetions.BaseDomain = _options.Value.OpenAIOptions.OpenAI.Gpt4Url ;
            }
                var openAiService = new OpenAIService(openAiOpetions);
            //调用SDK
            var response = openAiService.ChatCompletion.CreateCompletionAsStream(new ChatCompletionCreateRequest
            {
                Messages = messegs,
                Model = req.Model ?? _options.Value.OpenAIOptions.OpenAI.ChatModel,
                MaxTokens = maxtoken,
                TopP = req.TopP,
                N = req.N,
                PresencePenalty = req.PresencePenalty,
                FrequencyPenalty = req.FrequencyPenalty,
                Stop = req.Stop,
                Temperature = req.Temperature,
                LogitBias = req.LogitBias,
            });
            //接口返回的完整内容
            string totalMsg = "";
            var chatRes = new ChatRes() { Role = "assistant", Message = totalMsg, Model = req.Model, ModelType = req.ModelType, ConversationId = req.ConversationId, CreateDate = DateTime.Now, UserId = req.UserId,Enable = true};
            await foreach (var itemMsg in response)
            {
                if (itemMsg != null)
                {
                    if (itemMsg.Successful == false)
                    {
                        _logger.Error(itemMsg.Error.Message);
                        yield return ApiResponse<ChatRes>.Fail(itemMsg.Error.Message);
                        yield break;
                    }
                    totalMsg += itemMsg?.Choices?.FirstOrDefault().Message.Content;
                    chatRes.Message = totalMsg;
                    chatRes.PromptTokens = itemMsg.Usage?.PromptTokens;
                    chatRes.CompletionTokens = itemMsg.Usage?.CompletionTokens;
                    chatRes.TotalTokens = itemMsg.Usage?.TotalTokens;
                    if (!string.IsNullOrEmpty(itemMsg.Model))
                        chatRes.Model = itemMsg?.Model;
                    if (itemMsg.Choices != null && itemMsg.Choices.FirstOrDefault() != null && itemMsg.Choices.FirstOrDefault().Message != null && !string.IsNullOrEmpty(itemMsg.Choices.FirstOrDefault().Message.Role))
                    {
                        chatRes.Role = itemMsg.Choices.FirstOrDefault().Message.Role;
                    }
                    Log.Information($"[时间：{DateTime.Now}]stream输出：{itemMsg?.Choices?.FirstOrDefault().Message.Content}");
                    yield return ApiResponse<ChatRes>.Success(chatRes);
                }
            }
            takesPrice += TokensPrice(totalMsg, req.Model ?? _options.Value.OpenAIOptions.OpenAI.ChatModel);
            
            var chatRecord = _mapper.Map<ChatRecord>(chatRes);
            await _dbContext.ChatRecords.AddAsync(chatRecord);
            _dbContext.SaveChanges();
            user = await getSysUser(req.UserId);
            user.Balance -= takesPrice;
            user.ModifyDate = DateTime.Now;
            _dbContext.SysUsers.Update(user);
            _dbContext.SaveChanges();
        }


        public async Task<ApiResponse<ChatRes>> ChatCompletion(ChatReq req) {
            //创建会话
            if (req.ConversationId == null || req.ConversationId == 0) {
                var conversation = await _dbContext.ChatConversations.AddAsync(new ChatConversation(req.Prompt.Length < 5 ? req.Prompt : $"{req.Prompt.Substring(0, 5)}...", req.UserId));
                await _dbContext.SaveChangesAsync();
                req.ConversationId = conversation.Entity.ConversationId;
            }

            var user = await getSysUser(req.UserId);
            //敏感词检测
            if (!await Sensitive(req)) {
                 return ApiResponse<ChatRes>.Fail("触发了敏感词");
            }
            if (user == null)
            {
                 return ApiResponse<ChatRes>.Fail("当前用户不存在");
            }
            //上下文
            List<ChatMessage> messegs = await BuildMsgList(req);
            decimal takesPrice = GetAskPrice(messegs, req.Model ?? _options.Value.OpenAIOptions.OpenAI.ChatModel);

            if (user.Balance == null || user.Balance <= takesPrice)
            {
                return ApiResponse<ChatRes>.Fail($"账号余额不足，请充值");
            }
            int maxtoken;
            switch (req.Model) {
                case "gpt-4":
                    maxtoken = (int)((req.MaxTokens !=null && req.MaxTokens<4000)? req.MaxTokens: 4000);
                    break;
                case "gpt-3.5-turbo-16k":
                    maxtoken = (int)((req.MaxTokens != null && req.MaxTokens < 4000) ? req.MaxTokens : 4000);
                    break;
                default:
                    maxtoken = (int)((req.MaxTokens != null && req.MaxTokens < _options.Value.OpenAIOptions.OpenAI.MaxTokens) ? req.MaxTokens : _options.Value.OpenAIOptions.OpenAI.MaxTokens) ;
                    break;
            }
            string apikey;
            if (req.Model == "gpt-4")
                apikey = _options.Value.OpenAIOptions.OpenAI.Gpt4Key;
            else
                apikey = req.Key;

            var openAiOpetions = new OpenAI.OpenAiOptions() {
                ApiKey = req.Key,
                BaseDomain = _options.Value.OpenAIOptions.OpenAI.BaseUrl
            };
            //gpt-4走的第三方
            if (req.Model == "gpt-4") {
                openAiOpetions.ApiKey = _options.Value.OpenAIOptions.OpenAI.Gpt4Key;
                openAiOpetions.BaseDomain = _options.Value.OpenAIOptions.OpenAI.Gpt4Url;
            }
            var openAiService = new OpenAIService(openAiOpetions);
            //调用SDK
            var response = openAiService.ChatCompletion.CreateCompletion(new ChatCompletionCreateRequest {
                Messages = messegs,
                Model = req.Model ?? _options.Value.OpenAIOptions.OpenAI.ChatModel,
                MaxTokens = maxtoken,
                TopP =req.TopP,
                N = req.N,
                PresencePenalty = req.PresencePenalty,
                FrequencyPenalty = req.FrequencyPenalty,
                Stop = req.Stop,
                Temperature = req.Temperature,
                LogitBias = req.LogitBias,
            });
            if (response == null)
                return ApiResponse<ChatRes>.Fail("接口调用失败");
            if (!response.Result.Successful) {
                _logger.Error($"接口调用失败，key：{req.Key},报错内容：{response.Result.Error.Message}");
                return ApiResponse<ChatRes>.Fail(response.Result.Error.Message);
            }

            var chatRes = new ChatRes() { Role = "assistant", Message = response.Result.Choices.FirstOrDefault().Message.Content, Model = req.Model, ModelType = req.ModelType, ConversationId = req.ConversationId, CreateDate = DateTime.Now, UserId = req.UserId, Enable = true };
            chatRes.PromptTokens = response.Result.Usage?.PromptTokens;
            chatRes.CompletionTokens = response.Result.Usage?.CompletionTokens;
            chatRes.TotalTokens = response.Result.Usage?.TotalTokens;
            
            var chatRecord = _mapper.Map<ChatRecord>(chatRes);
            await _dbContext.ChatRecords.AddAsync(chatRecord);
            takesPrice += TokensPrice(response.Result.Choices.FirstOrDefault().Message.Content, req.Model ?? _options.Value.OpenAIOptions.OpenAI.ChatModel);
            user.Balance -= takesPrice;
            _dbContext.SysUsers.Update(user);
            await _dbContext.SaveChangesAsync();
            return ApiResponse<ChatRes>.Success(chatRes);
        }
        public async Task<ApiResponse<bool>> DeleteChatRecord(long recordId, long? userId) {
            var record = await _dbContext.ChatRecords.FirstOrDefaultAsync(m => m.ChatRecordId == recordId && m.Enable == true);
            record?.Delete(userId);
            await _dbContext.SaveChangesAsync();
            return ApiResponse<bool>.Success(true);
        }

        public async Task<ApiResponse<PagedRes<ChatRes>>> ChatRecordList(ChatRecordReq req) {
            var currentUser = await getSysUser(req.UserId);
            var query = _dbContext.ChatRecords.Where(m => m.Enable == true && (string.IsNullOrEmpty(req.QueryString) || m.Message.Contains(req.QueryString)));
            if (!(currentUser.RoleId == 1 && req.ConversationId == 0)) {
                query.Where(m => m.UserId == req.UserId && m.ConversationId == req.ConversationId);
            }
            var total = await query.CountAsync();
            var item = await query.OrderByDescending(m => m.CreateDate).Skip((req.PageIndex - 1) * req.PageSize).Take(req.PageSize).OrderBy(m=>m.CreateDate).ToListAsync();
            var res = _mapper.Map<IEnumerable<ChatRes>>(item);
            return ApiResponse<PagedRes<ChatRes>>.Success(new PagedRes<ChatRes>(res, total, req.PageIndex, req.PageSize));
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
            var query = _dbContext.Sensitives.Where(m=>m.Enable==true && ( string.IsNullOrEmpty(page.QueryString) || m.Word.Contains(page.QueryString)));
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

        #region 会话列表
        public async Task<ApiResponse<bool>> AddChatConversation(string conversationName, long? userId) {
            await _dbContext.ChatConversations.AddAsync(new ChatConversation(conversationName, userId));
            await _dbContext.SaveChangesAsync();
            return ApiResponse<bool>.Success(true);
        }

        public async Task<ApiResponse<bool>> ChangeChatConversation(long conversationId, string conversationName, long? userId) {
            var sensitive = await _dbContext.ChatConversations.FirstOrDefaultAsync(m => m.ConversationId == conversationId && m.Enable == true);
            sensitive?.Change(conversationName, userId);
            await _dbContext.SaveChangesAsync();
            return ApiResponse<bool>.Success(true);
        }

        public async Task<ApiResponse<bool>> DeleteChatConversation(long conversationId, long? userId) {
            var sensitive = await _dbContext.ChatConversations.FirstOrDefaultAsync(m => m.ConversationId == conversationId && m.Enable == true);
            sensitive?.Delete(userId);
            await _dbContext.SaveChangesAsync();
            return ApiResponse<bool>.Success(true);
        }

        public async Task<ApiResponse<PagedRes<ChatConversationRes>>> ChatConversationList(PageReq page, long? userId) {
            var query = _dbContext.ChatConversations.Where(m =>m.UserId==userId &&(  string.IsNullOrEmpty(page.QueryString) || m.ConversationName.Contains(page.QueryString)));
            var total = await query.CountAsync();
            var item = await query.Skip((page.PageIndex - 1) * page.PageSize).Take(page.PageSize).ToListAsync();
            var res = _mapper.Map<IEnumerable<ChatConversationRes>>(item);
            return ApiResponse<PagedRes<ChatConversationRes>>.Success(new PagedRes<ChatConversationRes>(res, total, page.PageIndex, page.PageSize));
        }
        #endregion

        #region 系统提示词
        /// <summary>
        /// 导入系统提示词(文件)
        /// </summary>
        /// <param name="file"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<ApiResponse<bool>> ImportPromptOptionByFile(IFormFile file, long? userId)
        {
            try
            {
                    // 打开文本文件
                    using (StreamReader sr = new StreamReader(file.OpenReadStream()))
                    {
                        string line=sr.ReadToEnd();

                        await ImportPromptOption(JsonSerializer.Deserialize<List<PromptOptionReq>>(line),userId);
                    }
                return ApiResponse<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error inserting data into PromptOption table");
                return ApiResponse<bool>.Fail(ex.ToString());
            }
        }

        /// <summary>
        /// 导入系统提示词
        /// </summary>
        /// <param name="prompts"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<ApiResponse<bool>> ImportPromptOption(List<PromptOptionReq> prompts, long? userId)
        {
            try
            {
                foreach (var item in prompts)
                {
                    _dbContext.PromptOptions.Add(new PromptOptions(item.Act,item.Prompt,userId));
                }
                await _dbContext.SaveChangesAsync();
                return ApiResponse<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.Fail(ex.ToString());
            }
        }
        /// <summary>
        /// 添加系统提示词
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<ApiResponse<bool>> AddPromptOption(PromptDetailReq req)
        {
            var prompt = new PromptOptions(req.Act, req.Prompt, req.UserId);
            await _dbContext.PromptOptions.AddAsync(prompt);
            await _dbContext.SaveChangesAsync();
            return ApiResponse<bool>.Success(true);
        }
        /// <summary>
        /// 修改系统提示词
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<ApiResponse<bool>> ChangePromptOption(PromptDetailReq req)
        {
            var PromptOption = await _dbContext.PromptOptions.FirstOrDefaultAsync(m => m.PromptId == req.PromptId && m.Enable == true);
            PromptOption?.Change(req.Act, req.Prompt,req.UserId);
            await _dbContext.SaveChangesAsync();
            return ApiResponse<bool>.Success(true);
        }
        /// <summary>
        /// 删除系统提示词
        /// </summary>
        /// <param name="promptId"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<ApiResponse<bool>> DeletePromptOption(long promptId, long? userId)
        {
            var PromptOption = await _dbContext.PromptOptions.FirstOrDefaultAsync(m => m.PromptId == promptId && m.Enable == true);
            PromptOption?.Delete(userId);
            await _dbContext.SaveChangesAsync();
            return ApiResponse<bool>.Success(true);
        }
        /// <summary>
        /// 系统提示词列表
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<ApiResponse<PagedRes<PromptOptionRes>>> PromptOptionList(PageReq page)
        {
            var query = _dbContext.PromptOptions.Where(m => string.IsNullOrEmpty(page.QueryString) || m.Act.Contains(page.QueryString) || m.Prompt.Contains(page.QueryString));
            var total = await query.CountAsync();
            var item = await query.Skip((page.PageIndex - 1) * page.PageSize).Take(page.PageSize).ToListAsync();
            var res = _mapper.Map<IEnumerable<PromptOptionRes>>(item);
            return ApiResponse<PagedRes<PromptOptionRes>>.Success(new PagedRes<PromptOptionRes>(res, total, page.PageIndex, page.PageSize));
        }
        #endregion

        #region 私有方法
        /// <summary>
        /// 敏感词检测
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        private async Task<bool> Sensitive(ChatReq req)
        {
            bool res = true;
            if (await _dbContext.Sensitives.CountAsync(m => req.Prompt.Contains(m.Word)) > 0)
            {
                //保存提问信息
                var problem = new ChatRecord();
                problem.IP = req.IP;
                problem.ConversationId = req.ConversationId;
                problem.Message = "敏感提问：" + req.Prompt;
                problem.Role = "user";
                problem.CreateDate = DateTime.Now;
                problem.UserId = req.UserId;
                problem.ModelType = req.ModelType ?? ((int)ModelTypeEnum.ChatGpt);
                problem.Model = req.Model ?? _options.Value.OpenAIOptions.OpenAI.ChatModel;
                _dbContext.ChatRecords.Add(problem);
                await _dbContext.SaveChangesAsync();
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
            return await _dbContext.SysUsers.AsNoTracking().FirstOrDefaultAsync(m => m.UserId == userId);
            //return await _helper.GetOrCreateAsync($"SysUser_{userId}", async options => { return await _dbContext.SysUsers.FirstOrDefaultAsync(m => m.UserId == userId); });
        }
        /// <summary>
        /// 获取当前用户当日提问次数
        /// </summary>
        /// <returns></returns>
        private async Task<int?> TodayVisits(long userId)
        {
            return await _dbContext.ChatRecords.CountAsync(m => m.UserId == userId && m.CreateDate.Date == DateTime.Now.Date && m.Role == "user");
            //return await _helper.GetOrCreateAsync($"TodayVisits_{userId}", async options => { return await _dbContext.ChatRecords.CountAsync(m => m.UserId == userId && m.CreateDate.Date == DateTime.Now.Date && m.Role == "user"); });
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
            var todays = await TodayVisits(user.UserId);
            if (todays > max)
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

        /// <summary>
        /// 构建对话信息
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        private async Task<List<ChatMessage>> BuildMsgList(ChatReq req)
        {
            //根据配置中的CONTEXT_COUNT 查询上下文
            var messegs = new List<ChatMessage>();
            if (!string.IsNullOrEmpty(req.SystemMessage))
            {
                messegs.Add(ChatMessage.FromSystem(req.SystemMessage));
            }

            var parentMessages = _dbContext.ChatRecords
                .Where(m => m.UserId == req.UserId && m.Enable == true && m.ConversationId == req.ConversationId)
                .OrderByDescending(m => m.CreateDate)
                .Take(req.ContextCount ?? _options.Value.OpenAIOptions.OpenAI.ContextCount).OrderBy(m => m.ChatRecordId)
                .ToList();
            //保证前后顺序，用for而不用foreach
            for (int i = 0; i < parentMessages.Count; i++)
            {
                switch (parentMessages[i].Role)
                {
                    case "assistant":
                        messegs.Add(ChatMessage.FromAssistant(parentMessages[i].Message));
                        break;
                    case "system":
                        messegs.Add(ChatMessage.FromSystem(parentMessages[i].Message));
                        break;
                    case "user":
                        messegs.Add(ChatMessage.FromUser(parentMessages[i].Message));
                        break;
                }
            }

            //当前问题
            messegs.Add(ChatMessage.FromUser(req.Prompt));
            _logger.Information(
                $"[{DateTime.Now.ToString()}]{req.UserId} 提问内容：{JsonSerializer.Serialize(req.Prompt, new JsonSerializerOptions()
                {
                    Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
                })}");
            var chat = new ChatRecord()
            {
                Role = "user", Message = req.Prompt, Model = req.Model, ModelType = req.ModelType,
                ConversationId = req.ConversationId, CreateDate = DateTime.Now, UserId = req.UserId, Enable = true
            };
            await _dbContext.ChatRecords.AddAsync(chat);
            _dbContext.SaveChanges();
            return messegs;
        }

        /// <summary>
        /// 根据模型和字符串内容计算消耗金额
        /// </summary>
        /// <param name="str"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        private decimal TokensPrice(string str, string model)
        {
            var length = System.Text.Encoding.Default.GetBytes(str.ToCharArray()).Length;
            decimal price = _options.Value.OpenAIOptions.TokenPrice > 0 ? _options.Value.OpenAIOptions.TokenPrice : (decimal)0.0001;
            switch (model)
            {
                case "gpt-3.5-turbo-16k":
                    price = price * 2;
                    break;
                case "gpt-4":
                    price = price * 3;
                    break;
                default: break;
            }
            return length * price;
        }

        private decimal GetAskPrice(List<ChatMessage> chatMessages, string model)
        {
            string msg = "";
            foreach (var item in chatMessages)
            {
                msg += item.Content;
            }
            return TokensPrice(msg, model);
        }
        #endregion
    }
}
