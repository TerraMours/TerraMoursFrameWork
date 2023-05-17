using AutoMapper;
using Microsoft.EntityFrameworkCore;
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
            var MenuList = await _dbContext.SysMenus.Where(m => m.Enable == true).ToListAsync();
            List<SysMenuRes> sysMenuRes = _mapper.Map<List<SysMenuRes>>(MenuList);
            return ApiResponse<List<SysMenuRes>>.Success(sysMenuRes);
        }
        /// <summary>
        /// 更新（目前只更新名称）
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<ApiResponse<bool>> UpdateMenu(SysMenuReq req)
        {
            var Menu = await _dbContext.SysMenus.FirstOrDefaultAsync(m => m.MenuId == req.MenuId);
            if (Menu == null)
            {
                return ApiResponse<bool>.Fail("用户不存在");
            }
            Menu.Change(_mapper,req);
            _dbContext.SaveChanges();
            return ApiResponse<bool>.Success(true);
        }
    }
}
