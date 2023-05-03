using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TerraMours.Framework.Infrastructure.Contracts.SystemModels
{
    public class SysMenuButtonsConfig : IEntityTypeConfiguration<SysMenuButtons>
    {
        public void Configure(EntityTypeBuilder<SysMenuButtons> builder)
        {
            //在 PostgreSQL 中，表名以及列名是不区分大小写的。这意味着在数据库中创建表时，无论您是使用大写、小写或混合大小写的名称，最终都会使用相同的名称来访问和操作该表。
            //设置表
            builder.ToTable("SysMenuButtons");
            //设置表主键
            builder.HasKey(e => e.MenuButtonId);
            //设置主键自增
            builder.Property(e => e.MenuButtonId)
                   .UseIdentityColumn();

            // 配置角色关系一个菜单对应的对应多个按钮 关系 外键是 MenuId
            builder.HasOne<SysRolesToMenu>(e => e.SysRolesToMenu)
                .WithMany(e => e.SysMenuButtons)
                .HasForeignKey(e => e.MenuId);

            // 配置关系一个菜单页面对应的对应多个按钮 关系 外键是 MenuId
            builder.HasOne<SysMenus>(e => e.SysMenus)
                .WithMany(e => e.SysMenuButtons)
                .HasForeignKey(e => e.MenuId);
        }
    }
}
