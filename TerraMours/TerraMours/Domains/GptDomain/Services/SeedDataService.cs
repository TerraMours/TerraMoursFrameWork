using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TerraMours.Domains.LoginDomain.Contracts.Common;
using TerraMours.Domains.LoginDomain.Contracts.Res;
using TerraMours.Framework.Infrastructure.Contracts.Commons;
using TerraMours.Framework.Infrastructure.Contracts.SystemModels;
using TerraMours.Framework.Infrastructure.EFCore;
using TerraMours.Framework.Infrastructure.Utils;
using TerraMours_Gpt.Domains.GptDomain.IServices;

namespace TerraMours_Gpt.Domains.GptDomain.Services
{
    /// <summary>
    /// 种子数据
    /// </summary>
    public class SeedDataService : ISeedDataService
    {
        private readonly FrameworkDbContext _dbContext;
        private readonly Serilog.ILogger _logger;
        private readonly IOptionsSnapshot<SysSettings> _sysSettings;

        public SeedDataService(FrameworkDbContext dbContext, Serilog.ILogger logger, IOptionsSnapshot<SysSettings> sysSettings)
        {
            _dbContext = dbContext;
            _logger = logger;
            _sysSettings = sysSettings;
        }

        /// <summary>
        /// 初始化数据
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<ApiResponse<bool>> EnsureSeedData()
        {
            if (!await _dbContext.SysMenus.AnyAsync())
            {
                await _dbContext.SysMenus.AddRangeAsync(new[]
                {
                    new SysMenus(){MenuId=1,HasChildren=false,MenuName="数据看板",MenuUrl="/dashboard/analysis",
                    Icon="icon-park-outline:analysis",Version=1,Enable=true,CreateDate=DateTime.Now,CreateID=1,OrderNo=0,ExternalUrl=false,IsHome=true,IsShow=true},
                    new SysMenus(){MenuId=2,HasChildren=false,MenuName="用户管理",MenuUrl="/management/user",
                    Icon="ic:round-manage-accounts",Version=1,Enable=true,CreateDate=DateTime.Now,CreateID=1,OrderNo=1,ExternalUrl=false,IsHome=false,IsShow=true},
                    new SysMenus(){MenuId=3,HasChildren=false,MenuName="系统管理",
                    Icon="carbon:cloud-service-management",Version=1,Enable=true,CreateDate=DateTime.Now,CreateID=1,OrderNo=2,ExternalUrl=false,IsHome=false,IsShow=true},
                    new SysMenus(){MenuId=4,ParentId=3,HasChildren=false,MenuName="权限管理",MenuUrl="/management/auth",
                    Icon="ic:baseline-security",Version=1,Enable=true,CreateDate=DateTime.Now,CreateID=1,OrderNo=0,ExternalUrl=false,IsHome=false,IsShow=true},
                    new SysMenus(){MenuId=5,ParentId=3,HasChildren=false,MenuName="角色管理",MenuUrl="/management/role",
                    Icon="ic:baseline-security",Version=1,Enable=true,CreateDate=DateTime.Now,CreateID=1,OrderNo=1,ExternalUrl=false,IsHome=false,IsShow=true},
                    new SysMenus(){MenuId=6,ParentId=3,HasChildren=false,MenuName="菜单管理",MenuUrl="/management/route",
                    Icon="ic:baseline-security",Version=1,Enable=true,CreateDate=DateTime.Now,CreateID=1,OrderNo=2,ExternalUrl=false,IsHome=false,IsShow=true}
                });
            }
            if (!await _dbContext.SysRolesToMenus.AnyAsync())
            {
                await _dbContext.SysRolesToMenus.AddRangeAsync(new[]
                {
                    new SysRolesToMenu(1,1),
                    new SysRolesToMenu(1,2),
                    new SysRolesToMenu(1,3),
                    new SysRolesToMenu(1,4),
                    new SysRolesToMenu(1,5),
                    new SysRolesToMenu(1,6)
                });
            }
            if (!await _dbContext.SysRoles.AnyAsync())
            {

                await _dbContext.SysRoles.AddRangeAsync(new[]
                {
                    new SysRole("超级管理员"){RoleId=1}
                });
            }
            if (!await _dbContext.SysUsers.AnyAsync())
            {
                await _dbContext.SysUsers.AddRangeAsync(new[]
                {
                    new SysUser("terramours@163.com","terramours@163.com".EncryptDES(_sysSettings.Value.secret.Encrypt)){UserId=1,RoleId=1}
                } );
            }
            await _dbContext.SaveChangesAsync();
            return ApiResponse<bool>.Success(true);
        }
    }
}
