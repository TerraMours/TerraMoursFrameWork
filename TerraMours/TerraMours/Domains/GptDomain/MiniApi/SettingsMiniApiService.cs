using Essensoft.Paylink.Alipay;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using TerraMours.Framework.Infrastructure.Contracts.Commons;
using TerraMours_Gpt.Domains.GptDomain.IServices;
using TerraMours_Gpt.Framework.Infrastructure.Contracts.Commons;

namespace TerraMours_Gpt.Domains.GptDomain.MiniApi
{
    public class SettingsMiniApiService : ServiceBase
    {
        private readonly ISeedDataService _seedDataService;
        private readonly ISettingsService _settingsService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public SettingsMiniApiService(ISeedDataService seedDataService, ISettingsService settingsService, IHttpContextAccessor httpContextAccessor) : base()
        {
            _seedDataService = seedDataService;
            _settingsService = settingsService;

            App.MapGet("/api/v1/Settings/EnsureSeedData", EnsureSeedData);
            App.MapGet("/api/v1/Settings/GetEmailSettings", GetEmailSettings);
            App.MapPost("/api/v1/Settings/ChangeEmailSettings", ChangeEmailSettings);
            App.MapGet("/api/v1/Settings/GetOpenAIOptions", GetOpenAIOptions);
            App.MapPost("/api/v1/Settings/ChangeOpenAIOptions", ChangeOpenAIOptions);
            App.MapGet("/api/v1/Settings/GetImagOptions", GetImagOptions);
            App.MapPost("/api/v1/Settings/ChangeImagOptions", ChangeImagOptions);
            App.MapGet("/api/v1/Settings/GetAlipayOptions", GetAlipayOptions);
            App.MapPost("/api/v1/Settings/ChangeAlipayOptions", ChangeAlipayOptions);
            _httpContextAccessor = httpContextAccessor;
        }

        /// <summary>
        /// 初始化数据库数据
        /// </summary>
        /// <returns></returns>
        public async Task<IResult> EnsureSeedData()
        {
            var res= await _seedDataService.EnsureSeedData();
            return Results.Ok(res);
        }

        /// <summary>
        /// 获取邮箱设置
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public async Task<IResult> GetEmailSettings()
        {
            var res = await _settingsService.GetEmailSettings();
            return Results.Ok(res);
        }

        /// <summary>
        /// 修改邮箱设置
        /// </summary>
        /// <param name="email">邮箱信息</param>
        /// <returns></returns>
        [Authorize]
        public async Task<IResult> ChangeEmailSettings(Email email)
        {
            var userId = long.Parse(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.UserData)!);
            var res = await _settingsService.ChangeEmailSettings(email,userId);
            return Results.Ok(res);
        }

        /// <summary>
        /// 获取AI配置
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public async Task<IResult> GetOpenAIOptions()
        {
            var res = await _settingsService.GetOpenAIOptions();
            return Results.Ok(res);
        }

        /// <summary>
        /// 修改AI配置
        /// </summary>
        /// <param name="email">邮箱信息</param>
        /// <returns></returns>
        [Authorize]
        public async Task<IResult> ChangeOpenAIOptions(OpenAIOptions openAIOptions)
        {
            var userId = long.Parse(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.UserData)!);
            var res = await _settingsService.ChangeOpenAIOptions(openAIOptions, userId);
            return Results.Ok(res);
        }

        /// <summary>
        /// 获取图片生成配置
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public async Task<IResult> GetImagOptions()
        {
            var res = await _settingsService.GetImagOptions();
            return Results.Ok(res);
        }

        /// <summary>
        /// 修改图片生成配置
        /// </summary>
        /// <param name="email">邮箱信息</param>
        /// <returns></returns>
        [Authorize]
        public async Task<IResult> ChangeImagOptions(ImagOptions imagOptions)
        {
            var userId = long.Parse(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.UserData)!);
            var res = await _settingsService.ChangeImagOptions(imagOptions, userId);
            return Results.Ok(res);
        }

        /// <summary>
        /// 获取支付配置
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public async Task<IResult> GetAlipayOptions() {
            var res = await _settingsService.GetAlipayOptions();
            return Results.Ok(res);
        }

        /// <summary>
        /// 修改支付配置
        /// </summary>
        /// <param name="email">邮箱信息</param>
        /// <returns></returns>
        [Authorize]
        public async Task<IResult> ChangeAlipayOptions(AlipayOptions alipayOptions) {
            var userId = long.Parse(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.UserData)!);
            var res = await _settingsService.ChangeAlipayOptions(alipayOptions, userId);
            return Results.Ok(res);
        }
    }
}
