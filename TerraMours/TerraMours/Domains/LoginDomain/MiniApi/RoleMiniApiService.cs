using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TerraMours.Domains.LoginDomain.Contracts.Req;
using TerraMours.Domains.LoginDomain.IServices;
using TerraMours.Domains.LoginDomain.Services;

namespace TerraMours.Domains.LoginDomain.MiniApi
{
    public class RoleMiniApiService:ServiceBase
    {
        private readonly ISysRoleService _sysRoleService;

        public RoleMiniApiService(ISysRoleService roleService) : base()
        {
            _sysRoleService = roleService;
            App.MapGet("/api/v1/Role/GetAllRoleList", GetAllRoleList);
            App.MapPost("/api/v1/Role/DelRole", DelRole);
            App.MapPost("/api/v1/Role/UpdateRole", UpdateRole);
            App.MapPost("/api/v1/Role/AddRole", AddRole);
            App.MapPost("/api/v1/Menu/GetRoleSelect", GetRoleSelect);
        }
        /// <summary>
		/// 全部角色列表 todo：jwt添加权限
		/// </summary>
		/// <returns></returns>
		[Authorize]
        public async Task<IResult> GetAllRoleList()
        {
            var res = await _sysRoleService.GetAllRoleList();
            return Results.Ok(res);
        }
        /// <summary>
        /// 删除角色
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [Authorize]
        public async Task<IResult> DelRole([FromBody] SysRoleBaseReq req)
        {
            var res = await _sysRoleService.DelRole(req);
            return Results.Ok(res);
        }

        /// <summary>
        /// 修改角色信息
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [Authorize]
        public async Task<IResult> UpdateRole([FromBody] SysRoleBaseReq req)
        {
            var res = await _sysRoleService.UpdateRole(req);
            return Results.Ok(res);
        }
        /// <summary>
        /// 新增角色（管理员）
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [Authorize]
        public async Task<IResult> AddRole([FromBody] SysRoleBaseReq req)
        {
            var res = await _sysRoleService.AddRole(req);
            return Results.Ok(res);
        }
        /// <summary>
        /// 菜单下拉框
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [Authorize]
        public async Task<IResult> GetRoleSelect()
        {
            var res = await _sysRoleService.GetRoleSelect();
            return Results.Ok(res);
        }
    }
}
