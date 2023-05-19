using AutoMapper;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Ocsp;
using StackExchange.Redis;
using System.Collections.Generic;
using TerraMours.Domains.LoginDomain.Contracts.Common;
using TerraMours.Domains.LoginDomain.Contracts.Req;
using TerraMours.Domains.LoginDomain.Contracts.Res;
using TerraMours.Domains.LoginDomain.IServices;
using TerraMours.Framework.Infrastructure.Contracts.SystemModels;
using TerraMours.Framework.Infrastructure.EFCore;

namespace TerraMours.Domains.LoginDomain.Services
{
    public class SysMenuService:ISysMenuService
    {
        private readonly FrameworkDbContext _dbContext;
        private readonly IMapper _mapper;
        public SysMenuService(FrameworkDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }
        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<ApiResponse<bool>> AddMenu(SysMenuReq req)
        {
            var menu = new SysMenus(_mapper,req);
            _dbContext.SysMenus.Add(menu);
            await _dbContext.SaveChangesAsync();
            return ApiResponse<bool>.Success(true);
        }

        /// <summary>
        /// 给角色配置菜单
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<ApiResponse<bool>> AddMenuToRole(MenuToRoleAddReq req)
        {
            //角色已有的菜单
            var UserMenusIds=await _dbContext.SysRolesToMenus.Where(m=>m.RoleId==req.RoleId && m.Enable==true).Select(m=>m.MenuId).ToListAsync();
            //角色新选择的菜单ids
            var newMenuIds = req.MenuIds.Where(m => !UserMenusIds.Contains(m)).ToList();
            //删除的ids
            var delMenuIds = UserMenusIds.Where(m => !req.MenuIds.Contains(m)).ToList();
            //删除菜单
            var DelMenu=await _dbContext.SysRolesToMenus.Where(m=> m.Enable == true && m.RoleId == req.RoleId && delMenuIds.Contains(m.MenuId)).ToListAsync();
            DelMenu.ForEach(m => m.Delete());
            //新增菜单
            newMenuIds.ForEach(async m =>await _dbContext.SysRolesToMenus.AddAsync(new SysRolesToMenu((long)req.RoleId, m)));
            await _dbContext.SaveChangesAsync();
            return ApiResponse<bool>.Success(true);
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<ApiResponse<bool>> DelMenu(SysMenuBaseReq req)
        {
            var Menu = await _dbContext.SysMenus.FirstOrDefaultAsync(m => m.MenuId == req.MenuId);
            Menu?.Delete();
            await _dbContext.SaveChangesAsync();
            return ApiResponse<bool>.Success(true);
        }
        /// <summary>
        /// 角色列表
        /// </summary>
        /// <returns></returns>
        public async Task<ApiResponse<List<SysMenuRes>>> GetAllMenuList()
        {
            var menuList = await _dbContext.SysMenus.Where(m => m.Enable == true).ToListAsync();
            List<SysMenuRes> sysMenuRes = _mapper.Map<List<SysMenuRes>>(menuList);
            return ApiResponse<List<SysMenuRes>>.Success(sysMenuRes);
        }
        /// <summary>
        /// 菜单树
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<ApiResponse<List<SysMenuRes>>> GetMenuTree(long? roleId)
        {
            var menusModels = await _dbContext.SysMenus.ToListAsync(); // 获取所有菜单
            //角色已有的菜单
            var userMenusIds = await _dbContext.SysRolesToMenus.Where(m => m.RoleId == roleId && m.Enable == true).Select(m => m.MenuId).ToListAsync();
            var menus = _mapper.Map<List<SysMenuRes>>(menusModels);
            foreach (var menu in menus.Where(m => userMenusIds.Contains(m.MenuId)))
            {
                menu.IsChecked = true;//设置已勾选
            }
            var roots = menus.Where(m => m.ParentId == null); // 获取所有根菜单
            foreach (var root in roots)
            {
                // 递归查询菜单的子菜单
                root.Children = FindChildren(root.MenuId, menus);
            }
            return ApiResponse<List<SysMenuRes>>.Success(roots.ToList());
        }
        /// <summary>
        /// 角色下的菜单id（因为前端的选中）
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<ApiResponse<List<long>>> GetRoleMenusIds(MenuToRoleAddReq req)
        {
            var menuIds =await _dbContext.SysRolesToMenus.Where(m => m.RoleId == req.RoleId && m.Enable == true).Select(m=>m.MenuId).ToListAsync();
            return ApiResponse<List<long>>.Success(menuIds);
        }

        /// <summary>
        /// 更新（目前只更新名称）
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<ApiResponse<bool>> UpdateMenu(SysMenuReq req)
        {
            var menu = await _dbContext.SysMenus.FirstOrDefaultAsync(m => m.MenuId == req.MenuId);
            if (menu == null)
            {
                return ApiResponse<bool>.Fail("用户不存在");
            }
            menu.Change(_mapper,req);
            _dbContext.SaveChanges();
            return ApiResponse<bool>.Success(true);
        }

        #region 私有方法
        /// <summary>
        /// 递归查询子集
        /// </summary>
        /// <param name="parentId"></param>
        /// <param name="menus"></param>
        /// <returns></returns>
        private List<SysMenuRes> FindChildren(long parentId, List<SysMenuRes> menus)
        {
            var children = menus.Where(m => m.ParentId == parentId).ToList();

            foreach (var child in children)
            {
                child.Children = FindChildren(child.MenuId, menus);
            }
            return children;
        }
        #endregion
    }
}
