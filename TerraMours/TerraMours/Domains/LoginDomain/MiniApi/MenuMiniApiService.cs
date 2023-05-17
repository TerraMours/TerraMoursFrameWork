using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TerraMours.Domains.LoginDomain.Contracts.Req;
using TerraMours.Domains.LoginDomain.IServices;
using TerraMours.Domains.LoginDomain.Services;

namespace TerraMours.Domains.LoginDomain.MiniApi
{
    /// <summary>
    /// 菜单管理
    /// </summary>
    public class MenuMiniApiService:ServiceBase
    {
        private readonly ISysMenuService _sysMenuService;

        public MenuMiniApiService(ISysMenuService menuService):base()
        {
            _sysMenuService = menuService;
            App.MapGet("/api/v1/Menu/GetAllMenuList", GetAllMenuList);
            App.MapPost("/api/v1/Menu/DelMenu", DelMenu);
            App.MapPost("/api/v1/Menu/UpdateMenu", UpdateMenu);
            App.MapPost("/api/v1/Menu/AddMenu", AddMenu);
        }
        /// <summary>
		/// 全部菜单列表 todo：jwt添加权限
		/// </summary>
		/// <returns></returns>
		[Authorize]
        public async Task<IResult> GetAllMenuList()
        {
            var res = await _sysMenuService.GetAllMenuList();
            return Results.Ok(res);
        }
        /// <summary>
        /// 删除菜单
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [Authorize]
        public async Task<IResult> DelMenu([FromBody] SysMenuBaseReq req)
        {
            var res = await _sysMenuService.DelMenu(req);
            return Results.Ok(res);
        }

        /// <summary>
        /// 修改菜单信息
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [Authorize]
        public async Task<IResult> UpdateMenu([FromBody] SysMenuReq req)
        {
            var res = await _sysMenuService.UpdateMenu(req);
            return Results.Ok(res);
        }
        /// <summary>
        /// 新增菜单（管理员）
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [Authorize]
        public async Task<IResult> AddMenu([FromBody] SysMenuReq req)
        {
            var res = await _sysMenuService.AddMenu(req);
            return Results.Ok(res);
        }
    }
}
