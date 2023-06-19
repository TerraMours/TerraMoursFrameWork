using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using TerraMours.Framework.Infrastructure.Contracts.Commons;
using TerraMours_Gpt.Domains.GptDomain.IServices;

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
    }
}
