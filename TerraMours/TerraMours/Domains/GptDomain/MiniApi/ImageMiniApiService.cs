using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using TerraMours_Gpt.Domains.GptDomain.Contracts.Req;
using TerraMours_Gpt.Domains.GptDomain.IServices;
using TerraMours_Gpt.Domains.GptDomain.Services;
using TerraMours_Gpt.Domains.LoginDomain.Contracts.Common;

namespace TerraMours_Gpt.Domains.GptDomain.MiniApi {
    public class ImageMiniApiService : ServiceBase {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IImageService _imageService;

        public ImageMiniApiService(IHttpContextAccessor httpContextAccessor, IImageService imageService) : base() {
            _httpContextAccessor = httpContextAccessor;
            _imageService = imageService;
            App.MapPost("/api/v1/Image/GenerateGraph", GenerateGraph);
            App.MapGet("/api/v1/Image/ShareImage", ShareImage);
            App.MapPost("/api/v1/Image/ShareImageList", ShareImageList);
            App.MapPost("/api/v1/Image/MyImageList", MyImageList);
            App.MapPost("/api/v1/Image/AllImageList", AllImageList);
        }
        /// <summary>
        /// 生成图片
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [Authorize]
        public async Task<IResult> GenerateGraph(ImageReq req) {
            if (_httpContextAccessor.HttpContext?.Items["key"] != null) {
                req.Key = _httpContextAccessor.HttpContext?.Items["key"]?.ToString();
            }
            req.UserId = long.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.UserData));
            var res = await _imageService.GenerateGraph(req);
            return Results.Ok(res);
        }

        /// <summary>
        /// 公开图片
        /// </summary>
        /// <param name="ImageRecordId"></param>
        /// <returns></returns>
        [Authorize]
        public async Task<IResult> ShareImage(long ImageRecordId,bool IsPublic) {
            var userId = long.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.UserData));
            var res = await _imageService.ShareImage(ImageRecordId, IsPublic, userId);
            return Results.Ok(res);
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="ImageRecordId"></param>
        /// <returns></returns>
        [Authorize]
        public async Task<IResult> DeleteImage(long ImageRecordId) {
            var userId = long.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.UserData));
            var res = await _imageService.DeleteImage(ImageRecordId, userId);
            return Results.Ok(res);
        }

        /// <summary>
        /// 图片广场
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        [Authorize]
        public async Task<IResult> ShareImageList(PageReq page) {
            var userId = long.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.UserData));
            var res = await _imageService.ShareImageList(page);
            return Results.Ok(res);
        }

        /// <summary>
        /// 我的图片
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        [Authorize]
        public async Task<IResult> MyImageList(PageReq page) {
            var userId = long.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.UserData));
            var res = await _imageService.MyImageList(page, userId);
            return Results.Ok(res);
        }
        /// <summary>
        /// 全部图片
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        [Authorize]
        public async Task<IResult> AllImageList(PageReq page) {
            var userId = long.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.UserData));
            var res = await _imageService.MyImageList(page, userId);
            return Results.Ok(res);
        }
    }
}
