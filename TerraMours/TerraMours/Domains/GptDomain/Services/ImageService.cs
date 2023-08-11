using Hangfire;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel.AI.ChatCompletion;
using Microsoft.SemanticKernel;
using Newtonsoft.Json;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using TerraMours.Domains.LoginDomain.Contracts.Common;
using TerraMours.Framework.Infrastructure.EFCore;
using TerraMours_Gpt.Domains.GptDomain.Contracts.Req;
using TerraMours_Gpt.Domains.GptDomain.Contracts.Res;
using TerraMours_Gpt.Domains.GptDomain.Hubs;
using TerraMours_Gpt.Domains.GptDomain.IServices;
using TerraMours_Gpt.Framework.Infrastructure.Contracts.Commons;
using TerraMours_Gpt.Framework.Infrastructure.Contracts.GptModels;
using Microsoft.SemanticKernel.AI.ImageGeneration;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI.ChatCompletion;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using TerraMours_Gpt.Domains.LoginDomain.Contracts.Common;
using OpenAI.ObjectModels;
using OpenAI.ObjectModels.RequestModels;
using OpenAI.Managers;
using OpenAI;

namespace TerraMours_Gpt.Domains.GptDomain.Services
{
    public class ImageService : IImageService {
        private readonly FrameworkDbContext _dbContext;
        private readonly IOptionsSnapshot<GptOptions> _options;
        private readonly HttpClient _httpClient;
        private readonly IHubContext<GraphGenerationHub> _hubContext;
        private readonly Serilog.ILogger _logger;
        private readonly IMapper _mapper;

        public ImageService(FrameworkDbContext dbContext, IOptionsSnapshot<GptOptions> options, HttpClient httpClient, IHubContext<GraphGenerationHub> hubContext, Serilog.ILogger logger, IMapper mapper) {
            _dbContext = dbContext;
            _options = options;
            _httpClient = httpClient;
            _hubContext = hubContext;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<ApiResponse<string?>> GenerateGraph(ImageReq req) {

            //添加后台任务
            BackgroundJob.Enqueue(() => BackServiceCreateImg(req));
            string message = await SendWaitingCount("img-queue");
            return ApiResponse<string?>.Success(message);
        }

        public async Task<ApiResponse<bool>> ShareImage(long ImageRecordId, long? userId) {
            var image = await _dbContext.ImageRecords.FirstOrDefaultAsync(m => m.ImageRecordId == ImageRecordId && m.Enable == true);
            if (image == null) {
                return ApiResponse<bool>.Fail("图片不存在");
            }
            image.Share(userId);
            await _dbContext.SaveChangesAsync();
            return ApiResponse<bool>.Success(true);
        }


        public async Task<ApiResponse<PagedRes<ImageRes>>> ShareImageList(PageReq page) {
            var query = _dbContext.ImageRecords.Where(m => (string.IsNullOrEmpty(page.QueryString) || m.Prompt.Contains(page.QueryString)) && m.Enable == true && m.IsPublic == true);
            var total = await query.CountAsync();
            var item = await query.OrderByDescending(m => m.CreateDate).Skip((page.PageIndex - 1) * page.PageSize).Take(page.PageSize).ToListAsync();
            var res = _mapper.Map<IEnumerable<ImageRes>>(item);
            return ApiResponse<PagedRes<ImageRes>>.Success(new PagedRes<ImageRes>(res, total, page.PageIndex, page.PageSize));
        }

        public async Task<ApiResponse<PagedRes<ImageRes>>> MyImageList(PageReq page, long? userId) {
            var query = _dbContext.ImageRecords.Where(m => (string.IsNullOrEmpty(page.QueryString) || m.Prompt.Contains(page.QueryString)) && m.Enable==true && m.UserId==userId);
            var total = await query.CountAsync();
            var item = await query.OrderByDescending(m => m.CreateDate).Skip((page.PageIndex - 1) * page.PageSize).Take(page.PageSize).ToListAsync();
            var res = _mapper.Map<IEnumerable<ImageRes>>(item);
            return ApiResponse<PagedRes<ImageRes>>.Success(new PagedRes<ImageRes>(res, total, page.PageIndex, page.PageSize));
        }

        public async Task<ApiResponse<bool>> DeleteImage(long ImageRecordId, long? userId) {
            var image = await _dbContext.ImageRecords.FirstOrDefaultAsync(m => m.ImageRecordId == ImageRecordId && m.Enable == true);
            if (image == null) {
                return ApiResponse<bool>.Fail("图片不存在");
            }
            image.Delete(userId);
            await _dbContext.SaveChangesAsync();
            return ApiResponse<bool>.Success(true);
        }
        /// <summary>
        /// 后台任务生成图片（DisableConcurrentExecution 设置超时时间 Queue设置任务类型）
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [DisableConcurrentExecution(timeoutInSeconds: 180)]
        [Queue("img-queue")]
        public async Task BackServiceCreateImg(ImageReq request) {
            try {
                List<string> imgList = await CreatImages(request);
                _hubContext.Clients.Client(request.ConnectionId).SendAsync("updateImgUrl", imgList);
            }
            catch (Exception ex) {
                _logger.Information($"SD 生成图片报错：{ex.Message}");
                _hubContext.Clients.Client(request.ConnectionId).SendAsync("updateImgUrl", $"生成图片失败：{ex.Message}");
            }
            finally {
                SendWaitingCount("img-queue");
            }
        }
        /// <summary>
        /// 后台任务生成图片
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<List<string>> CreatImages(ImageReq request) {
            List<string> imgList = new List<string>();
            var prompt=request.Prompt;
            string pranslatePrompt= string.Empty;
            switch (request.ModelType) {
                case 0:
                    imgList = await CreateGptImgOpenAi(request);
                    break;
                case 1:
                default:
                    //判断用户输入的是不是中文，是中文调用chatgpt翻译
                    pranslatePrompt= await TranslateOpenAi(request);
                    request.Prompt = pranslatePrompt;
                    imgList = await CreateSDImg(request);
                    break;
            }

            if (imgList.Count() > 0) {
                foreach (var item in imgList)
                {
                    SaveImg(prompt, pranslatePrompt, item, request.ModelType, request.Model,request.Size, request.UserId);
                }
                await _dbContext.SaveChangesAsync();
            }
            return imgList;
        }
        #region 私有方法
        /// <summary>
        /// 查询用户图片次数
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        private int? ImgCount(long userId) {
            var user= _dbContext.SysUsers.FirstOrDefault(m=>m.UserId ==userId);
            return user?.ImageCount;
        }
        /// <summary>
        /// 保存记录
        /// </summary>
        /// <param name="prompt"></param>
        /// <param name="pranslatePrompt"></param>
        /// <param name="imagUrl"></param>
        /// <param name="modelType"></param>
        /// <param name="model"></param>
        /// <param name="size"></param>
        /// <param name="userId"></param>
        private void SaveImg(string? prompt, string? pranslatePrompt, string? imagUrl, int? modelType, string? model, int? size, long? userId) {
            ImageRecord record = new ImageRecord(prompt,pranslatePrompt,imagUrl,modelType,model,size,userId);
            _dbContext.ImageRecords.Add(record);
        }

        /// <summary>
        /// sd图片
        /// </summary>
        /// <param name="form"></param>
        /// <returns></returns>
        private async Task<List<string>> CreateSDImg(ImageReq form) {
            _logger.Information("CreateSDImg");
            SDOptions sDOptions = new SDOptions();
            var imageOption = await _dbContext.GptOptions.FirstOrDefaultAsync();
            if(imageOption ==null)
            {
                sDOptions = _options.Value.ImagOptions.SDOptions.FirstOrDefault();
            }
            else
            {
                sDOptions = form.Model != null ? imageOption.ImagOptions.SDOptions?.FirstOrDefault(m => m.Label == form.Model) : imageOption.ImagOptions.SDOptions?.FirstOrDefault();
            }
            var httpClient = new HttpClient();
            SDImgReq dto = new SDImgReq();
            dto.prompt = form.Prompt;
            dto.steps = 20;
            dto.batch_size = form.Count;
            dto.negative_prompt =form.NegativePrompt ?? sDOptions?.Negative_Prompt ?? "wrong hands";
            var requestUrl = $"{sDOptions?.BaseUrl}/sdapi/v1/txt2img";
            var content = new StringContent(JsonConvert.SerializeObject(dto), Encoding.UTF8, "application/json");
            _logger.Information($"调用SD api，url:{requestUrl},参数：{await content.ReadAsStringAsync()}");
            var message = await httpClient.PostAsync(requestUrl, content);
            _logger.Information($"调用SD api，返回结果：{await new StringContent(JsonConvert.SerializeObject(message), Encoding.UTF8, "application/json").ReadAsStringAsync()}");
            SDImgRes response = await message.Content.ReadFromJsonAsync<SDImgRes>();
            //保存文件
            var imgs = response?.images;
            var imgList = new List<string>();
            for (int i = 0; i < imgs.Length; i++) {
                var fileName = $"{Guid.NewGuid()}.png";
                var folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images");
                if (!Directory.Exists(folderPath)) {
                    Directory.CreateDirectory(folderPath);
                }
                var filePath = Path.Combine(folderPath, fileName);
                // 将 Base64 数据转换为二进制数据，并写入文件
                var imageData = Convert.FromBase64String(imgs[i]);
                await System.IO.File.WriteAllBytesAsync(filePath, imageData);
                // 获取图片路径
                var baseUrl = _options.Value.ImagOptions.ImagFileBaseUrl;
                // 生成图片 URL，注意将反斜杠转换为正斜杠
                var imageUrl = $"{baseUrl}/{fileName.Replace("\\", "/")}";
                imgList.Add(imageUrl);
            }
            return imgList;
        }

        /// <summary>
        /// chatgpt生成图片
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        private async Task<List<string>> CreateGptImg(ImageReq req) {
            IKernel kernel = Kernel.Builder.Build();
            kernel.Config.AddOpenAIImageGenerationService("dallE",
            req.Key);
            IImageGeneration dallE = kernel.GetService<IImageGeneration>();
            var imgList = new List<string>();
            for (int i = 0; i < req.Count; i++) {
                var content = await dallE.GenerateImageAsync(req.Prompt, (int)req.Size, (int)req.Size);
                _logger.Information("生成图片结果：" + content);
                imgList.Add(content);
            }
            return imgList;
        }

        /// <summary>
        /// chatgpt生成图片(OpenAi)
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        private async Task<List<string>> CreateGptImgOpenAi(ImageReq req)
        {
            var size = StaticValues.ImageStatics.Size.Size512;
            switch (req.Size)
            {
                case 256:
                    size = StaticValues.ImageStatics.Size.Size256; break;
                case 1024:
                    size = StaticValues.ImageStatics.Size.Size1024; break;
                default:
                    size = StaticValues.ImageStatics.Size.Size1024; break;
            }
            var openAiService = new OpenAIService(new OpenAiOptions()
            {
                ApiKey = req.Key,
                BaseDomain = _options.Value.OpenAIOptions.OpenAI.BaseUrl
            });
            //接受传进来的prompt生成一张或者多张图片
            var imageResult = await openAiService.Image.CreateImage(new ImageCreateRequest
            {
                //提示词
                Prompt = req.Prompt,
                //生成图片数量
                N = req.Count,
                Size = size,
                //返回url或者base64,url更合适 
                ResponseFormat = StaticValues.ImageStatics.ResponseFormat.Url,
                User = "user"
            });
            string jsonContent = System.Text.Json.JsonSerializer.Serialize(imageResult, new JsonSerializerOptions
            {
                IgnoreNullValues = true
            });
            _logger.Information("生成图片结果：" + jsonContent);
            List<string> res = new List<string>();
            if (imageResult.Successful)
            {
                foreach (var item in imageResult.Results)
                {
                    res.Add(item.Url);
                }
            }
            return res;
        }
        /// <summary>
        /// 翻译文本
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        private async Task<string> Translate(ImageReq req) {
            // 定义正则表达式
            Regex regex = new Regex(@"[\u4e00-\u9fa5]+");
            if (!regex.IsMatch(req.Prompt)) {
                _logger.Information("SD prompt无需翻译。");
                return req.Prompt;
            }
            IKernel kernel = Kernel.Builder.Build();
            kernel.Config.AddOpenAIChatCompletionService(_options.Value.OpenAIOptions.OpenAI.ChatModel,
            req.Key);
            var chatCompletion = kernel.GetService<IChatCompletion>();
            var options = new ChatRequestSettings() {
                MaxTokens = _options.Value.OpenAIOptions.OpenAI.MaxTokens,
                Temperature = _options.Value.OpenAIOptions.OpenAI.Temperature,
                FrequencyPenalty = _options.Value.OpenAIOptions.OpenAI.FrequencyPenalty,
                PresencePenalty = _options.Value.OpenAIOptions.OpenAI.PresencePenalty,
                TopP = _options.Value.OpenAIOptions.OpenAI.TopP
            };
            var chatHistory = new OpenAIChatHistory();
            chatHistory.AddSystemMessage("Translate Chinese into English");
            chatHistory.AddUserMessage(req.Prompt);
            var res=await chatCompletion.GenerateMessageAsync(chatHistory);
            return res;
        }

        /// <summary>
        /// 翻译文本openai
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        private async Task<string> TranslateOpenAi(ImageReq req)
        {
            // 定义正则表达式
            Regex regex = new Regex(@"[\u4e00-\u9fa5]+");
            if (!regex.IsMatch(req.Prompt))
            {
                _logger.Information("SD prompt无需翻译。");
                return req.Prompt;
            }
            var openAiService = new OpenAIService(new OpenAiOptions()
            {
                ApiKey = req.Key,
                BaseDomain = _options.Value.OpenAIOptions.OpenAI.BaseUrl
            });
            var messegs = new List<ChatMessage>();
            messegs.Add(ChatMessage.FromSystem("Translate Chinese into English"));
            messegs.Add(ChatMessage.FromUser(req.Prompt));
            //调用SDK
            var response = await openAiService.ChatCompletion.CreateCompletion(new ChatCompletionCreateRequest
            {
                Messages = messegs,
                Model = _options.Value.OpenAIOptions.OpenAI.ChatModel,
                MaxTokens = 500,
            });
            return response.Choices[0].Message.Content;
        }
        /// <summary>
        /// 推送队列的等待信息
        /// </summary>
        /// <param name="enqueue">任务类型</param>
        /// <returns></returns>
        private async Task<string> SendWaitingCount(string enqueue) {
            var queueLength = JobStorage.Current.GetMonitoringApi()
                            .EnqueuedCount(enqueue);
            string message = $"任务已提交，您前面还有 {queueLength} 个任务正在等待。";
            await _hubContext.Clients.All.SendAsync("updateWaitingCount", queueLength);
            return message;
        }

        #endregion
    }
}
