using Essensoft.Paylink.Alipay;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using TerraMours.Domains.LoginDomain.Contracts.Common;
using TerraMours.Domains.LoginDomain.Contracts.Res;
using TerraMours.Framework.Infrastructure.Contracts.Commons;
using TerraMours.Framework.Infrastructure.Contracts.SystemModels;
using TerraMours.Framework.Infrastructure.EFCore;
using TerraMours.Framework.Infrastructure.Utils;
using TerraMours_Gpt.Domains.GptDomain.IServices;
using TerraMours_Gpt.Framework.Infrastructure.Contracts.Commons;
using TerraMours_Gpt.Framework.Infrastructure.Contracts.GptModels;
using TerraMours_Gpt.Framework.Infrastructure.Contracts.SystemModels;

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
        private readonly IOptionsSnapshot<GptOptions> _gptOptions;
        private readonly IOptionsSnapshot<AlipayOptions> _alipayOptions;

        public SeedDataService(FrameworkDbContext dbContext, Serilog.ILogger logger, IOptionsSnapshot<SysSettings> sysSettings, IOptionsSnapshot<GptOptions> gptOptions, IOptionsSnapshot<AlipayOptions> alipayOptions)
        {
            _dbContext = dbContext;
            _logger = logger;
            _sysSettings = sysSettings;
            _gptOptions = gptOptions;
            _alipayOptions = alipayOptions;
        }

        /// <summary>
        /// 初始化数据
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<ApiResponse<bool>> EnsureSeedData()
        {
            // 清空数据库
            //await _dbContext.Database.ExecuteSqlRawAsync("DROP SCHEMA public CASCADE; CREATE SCHEMA public;");

            //await _dbContext.Database.MigrateAsync();
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
                    Icon="icon-park-outline:people-safe",Version=1,Enable=true,CreateDate=DateTime.Now,CreateID=1,OrderNo=1,ExternalUrl=false,IsHome=false,IsShow=true},
                    new SysMenus(){MenuId=6,ParentId=3,HasChildren=false,MenuName="菜单管理",MenuUrl="/management/route",
                    Icon="icon-park-outline:more-app",Version=1,Enable=true,CreateDate=DateTime.Now,CreateID=1,OrderNo=2,ExternalUrl=false,IsHome=false,IsShow=true},
                    new SysMenus(){MenuId=7,HasChildren=false,MenuName="聊天记录",MenuUrl="/management/chat",
                    Icon="icon-park-outline:adobe-illustrate",Version=1,Enable=true,CreateDate=DateTime.Now,CreateID=1,OrderNo=0,ExternalUrl=false,IsHome=false,IsShow=true},
                    new SysMenus(){MenuId=8,HasChildren=false,MenuName="数据看板",MenuUrl="/dashboard/analysis",
                    Icon="icon-park-outline:analysis",Version=1,Enable=true,CreateDate=DateTime.Now,CreateID=1,OrderNo=0,ExternalUrl=false,IsHome=false,IsShow=true},
                    new SysMenus(){MenuId=9,HasChildren=false,MenuName="敏感词管理",MenuUrl="/management/sensitive",
                    Icon="icon-park-outline:message-failed",Version=1,Enable=true,CreateDate=DateTime.Now,CreateID=1,OrderNo=0,ExternalUrl=false,IsHome=false,IsShow=true},
                    new SysMenus(){MenuId=10,HasChildren=false,MenuName="系统提示词",MenuUrl="/management/promptOption",
                    Icon="icon-park-outline:message-emoji",Version=1,Enable=true,CreateDate=DateTime.Now,CreateID=1,OrderNo=0,ExternalUrl=false,IsHome=false,IsShow=true},
                    new SysMenus(){MenuId=11,HasChildren=false,MenuName="Key池管理",MenuUrl="/management/keyOption",
                    Icon="icon-park-outline:file-settings-one",Version=1,Enable=true,CreateDate=DateTime.Now,CreateID=1,OrderNo=0,ExternalUrl=false,IsHome=false,IsShow=true},
                    new SysMenus(){MenuId=12,HasChildren=false,MenuName="商品管理",
                    Icon="icon-park-outline:workbench",Version=1,Enable=true,CreateDate=DateTime.Now,CreateID=1,OrderNo=3,ExternalUrl=false,IsHome=false,IsShow=true},
                     new SysMenus(){MenuId=13,ParentId=12,HasChildren=false,MenuName="商品分类",MenuUrl="/management/category",
                    Icon="icon-park-outline:buy",Version=1,Enable=true,CreateDate=DateTime.Now,CreateID=1,OrderNo=0,ExternalUrl=false,IsHome=false,IsShow=true},
                    new SysMenus(){MenuId=14,ParentId=12,HasChildren=false,MenuName="商品列表",MenuUrl="/management/product",
                    Icon="icon-park-outline:commodity",Version=1,Enable=true,CreateDate=DateTime.Now,CreateID=1,OrderNo=1,ExternalUrl=false,IsHome=false,IsShow=true},
                    new SysMenus(){MenuId=15,ParentId=3,HasChildren=false,MenuName="系统设置",MenuUrl="/management/settings",
                    Icon="icon-park-outline:setting",Version=1,Enable=true,CreateDate=DateTime.Now,CreateID=1,OrderNo=3,ExternalUrl=false,IsHome=false,IsShow=true},
                    new SysMenus(){MenuId=16,HasChildren=false,MenuName="图片记录管理",MenuUrl="/management/image",
                    Icon="icon-park-outline:adobe-photoshop",Version=1,Enable=true,CreateDate=DateTime.Now,CreateID=1,OrderNo=0,ExternalUrl=false,IsHome=false,IsShow=true},
                    new SysMenus(){MenuId=17,HasChildren=false,MenuName="图标选择",MenuUrl="/plugin/icon",
                    Icon="icon-park-outline:pic-one",Version=1,Enable=true,CreateDate=DateTime.Now,CreateID=1,OrderNo=0,ExternalUrl=false,IsHome=false,IsShow=true},
                    new SysMenus(){MenuId=18,HasChildren=false,MenuName="订单列表",MenuUrl="/management/order",
                    Icon="icon-park-outline:shopping",Version=1,Enable=true,CreateDate=DateTime.Now,CreateID=1,OrderNo=0,ExternalUrl=false,IsHome=false,IsShow=true}
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
                    new SysRolesToMenu(1,6),
                    new SysRolesToMenu(1,7),
                    new SysRolesToMenu(1,8),
                    new SysRolesToMenu(1,9),
                    new SysRolesToMenu(1,10),
                    new SysRolesToMenu(1,11),
                    new SysRolesToMenu(1,12),
                    new SysRolesToMenu(1,13),
                    new SysRolesToMenu(1,14),
                    new SysRolesToMenu(1,15),
                    new SysRolesToMenu(1,16),
                    new SysRolesToMenu(1,17),
                    new SysRolesToMenu(1,18),
                });
            }
            if (!await _dbContext.SysRoles.AnyAsync())
            {

                await _dbContext.SysRoles.AddRangeAsync(new[]
                {
                    new SysRole("超级管理员"){RoleId=1},
                    new SysRole("普通用户"){RoleId=2}
                });
            }
            if (!await _dbContext.SysUsers.AnyAsync())
            {
                await _dbContext.SysUsers.AddRangeAsync(new[]
                {
                    new SysUser("terramours@163.com","terramours@163.com".EncryptDES(_sysSettings.Value.secret.Encrypt)){UserId=1,RoleId=1,Gender="1"}
                } );
            }
            if (!await _dbContext.SysSettings.AnyAsync())
            {
                var settins=new SysSettingsEntity(_sysSettings.Value.initial, _sysSettings.Value.email,_alipayOptions.Value);
                await _dbContext.SysSettings.AddAsync(settins);
            }
            if (!await _dbContext.GptOptions.AnyAsync())
            {
                var settins = new GptOptionsEntity(_gptOptions.Value.OpenAIOptions, _gptOptions.Value.ImagOptions);
                await _dbContext.GptOptions.AddAsync(settins);
            }
            await _dbContext.SaveChangesAsync();
            return ApiResponse<bool>.Success(true);
        }
    }
}
