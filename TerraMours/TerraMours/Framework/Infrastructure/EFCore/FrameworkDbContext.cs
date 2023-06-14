using Microsoft.EntityFrameworkCore;
using TerraMours.Framework.Infrastructure.Contracts.SystemModels;
using TerraMours_Gpt.Framework.Infrastructure.Contracts.GptModels;

namespace TerraMours.Framework.Infrastructure.EFCore
{
    public class FrameworkDbContext : DbContext
    {
        //数据库表
        public DbSet<SysUser> SysUsers { get; set; }
        public DbSet<SysRole> SysRoles { get; set; }
        public DbSet<SysMenus> SysMenus { get; set; }
        public DbSet<SysMenuButtons> SysMenuButtons { get; set; }
        public DbSet<SysRolesToMenu> SysRolesToMenus { get; set; }
        #region 业务
        public DbSet<ChatRecord> ChatRecords { get; set; }
        public DbSet<Sensitive> Sensitives { get; set; }

        #endregion


        public FrameworkDbContext(DbContextOptions<FrameworkDbContext> options) : base(options)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(GetType().Assembly);
        }


    }
}
