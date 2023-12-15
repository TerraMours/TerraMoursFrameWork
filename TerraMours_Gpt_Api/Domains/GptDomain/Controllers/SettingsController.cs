﻿using Essensoft.Paylink.Alipay;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TerraMours.Framework.Infrastructure.Contracts.Commons;
using TerraMours_Gpt.Domains.GptDomain.IServices;
using TerraMours_Gpt.Framework.Infrastructure.Contracts.Commons;

namespace TerraMours_Gpt_Api.Domains.GptDomain.Controllers {
    [Route("api/v1/[controller]/[action]")]
    [ApiController]
    public class SettingsController : ControllerBase {
        private readonly ISeedDataService _seedDataService;
        private readonly ISettingsService _settingsService;

        public SettingsController(ISeedDataService seedDataService, ISettingsService settingsService) {
            _seedDataService = seedDataService;
            _settingsService = settingsService;
        }
        /// <summary>
        /// 初始化数据库数据
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IResult> EnsureSeedData() {
            var res = await _seedDataService.EnsureSeedData();
            return Results.Ok(res);
        }

        /// <summary>
        /// 获取邮箱设置
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [HttpGet]
        public async Task<IResult> GetEmailSettings() {
            var res = await _settingsService.GetEmailSettings();
            return Results.Ok(res);
        }

        /// <summary>
        /// 修改邮箱设置
        /// </summary>
        /// <param name="email">邮箱信息</param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        public async Task<IResult> ChangeEmailSettings(Email email) {
            var userId = long.Parse(HttpContext!.User.FindFirstValue(ClaimTypes.UserData)!);
            var res = await _settingsService.ChangeEmailSettings(email, userId);
            return Results.Ok(res);
        }

        /// <summary>
        /// 获取AI配置
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [HttpGet]
        public async Task<IResult> GetOpenAIOptions() {
            var res = await _settingsService.GetOpenAIOptions();
            return Results.Ok(res);
        }

        /// <summary>
        /// 修改AI配置
        /// </summary>
        /// <param name="email">邮箱信息</param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        public async Task<IResult> ChangeOpenAIOptions(OpenAIOptions openAIOptions) {
            var userId = long.Parse(HttpContext!.User.FindFirstValue(ClaimTypes.UserData)!);
            var res = await _settingsService.ChangeOpenAIOptions(openAIOptions, userId);
            return Results.Ok(res);
        }

        /// <summary>
        /// 获取图片生成配置
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [HttpGet]
        public async Task<IResult> GetImagOptions() {
            var res = await _settingsService.GetImagOptions();
            return Results.Ok(res);
        }

        /// <summary>
        /// 修改图片生成配置
        /// </summary>
        /// <param name="email">邮箱信息</param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        public async Task<IResult> ChangeImagOptions(ImagOptions imagOptions) {
            var userId = long.Parse(HttpContext!.User.FindFirstValue(ClaimTypes.UserData)!);
            var res = await _settingsService.ChangeImagOptions(imagOptions, userId);
            return Results.Ok(res);
        }

        /// <summary>
        /// 获取支付配置
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [HttpGet]
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
        [HttpPost]
        public async Task<IResult> ChangeAlipayOptions(AlipayOptions alipayOptions) {
            var userId = long.Parse(HttpContext!.User.FindFirstValue(ClaimTypes.UserData)!);
            var res = await _settingsService.ChangeAlipayOptions(alipayOptions, userId);
            return Results.Ok(res);
        }
    }
}
