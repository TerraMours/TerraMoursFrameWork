using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Ocsp;
using System.Security.Claims;
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
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MenuMiniApiService(ISysMenuService menuService, IHttpContextAccessor httpContextAccessor) : base()
        {
            _sysMenuService = menuService;
            App.MapGet("/api/v1/Menu/GetAllMenuList", GetAllMenuList);
            App.MapPost("/api/v1/Menu/DelMenu", DelMenu);
            App.MapPost("/api/v1/Menu/UpdateMenu", UpdateMenu);
            App.MapPost("/api/v1/Menu/AddMenu", AddMenu);
            App.MapPost("/api/v1/Menu/GetMenuTree", GetMenuTree);
            App.MapPost("/api/v1/Menu/AddMenuToRole", AddMenuToRole);
            App.MapPost("/api/v1/Menu/GetRoleMenusIds", GetRoleMenusIds);
            App.MapPost("/api/v1/Menu/GetUserRoutes", GetUserRoutes);
            App.MapPost("/api/v1/Menu/GetMenuSelect", GetMenuSelect);
            _httpContextAccessor = httpContextAccessor;
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
        /// <summary>
        /// 菜单树
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public async Task<IResult> GetMenuTree([FromBody] MenuToRoleAddReq req)
        {
            var res = await _sysMenuService.GetMenuTree(req.RoleId);
            return Results.Ok(res);
        }

        /// <summary>
        /// 给角色配置菜单
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public async Task<IResult> AddMenuToRole([FromBody] MenuToRoleAddReq req)
        {
            var res = await _sysMenuService.AddMenuToRole(req);
            return Results.Ok(res);
        }

        /// <summary>
        /// 角色下的菜单id（因为前端的选中）
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public async Task<IResult> GetRoleMenusIds([FromBody] MenuToRoleAddReq req)
        {
            var res = await _sysMenuService.GetRoleMenusIds(req);
            return Results.Ok(res);
        }
        /// <summary>
        /// 用户路由
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [Authorize]
        public async Task<IResult> GetUserRoutes([FromBody] SysMenuBaseReq req)
        {
            if(req.RoleId == null)
            {
                req.RoleId =long.Parse( _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Role));
            }
            var res =await _sysMenuService.GetUserRoutes(req.RoleId);
            return Results.Ok(res);
        }
        /// <summary>
        /// 菜单下拉框
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [Authorize]
        public async Task<IResult> GetMenuSelect([FromBody] SysMenuBaseReq req)
        {
            if (req.RoleId == null)
            {
                req.RoleId = long.Parse(_httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Role));
            }
            var res = await _sysMenuService.GetMenuSelect(req);
            return Results.Ok(res);
        }
    }
}
