﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TerraMours.Framework.Infrastructure.Contracts.SystemModels
{
    public class SysRolesToMenuConfig : IEntityTypeConfiguration<SysRolesToMenu>
    {
        public void Configure(EntityTypeBuilder<SysRolesToMenu> builder)
        {
            //在 PostgreSQL 中，表名以及列名是不区分大小写的。这意味着在数据库中创建表时，无论您是使用大写、小写或混合大小写的名称，最终都会使用相同的名称来访问和操作该表。
            //设置表
            builder.ToTable("SysRolesToMenu");
            //设置表主键
            builder.HasKey(e => e.RolesToMenuId);
            //设置主键自增
            builder.Property(e => e.RolesToMenuId)
                   .UseIdentityColumn();
            //将 Version 属性设置为每次插入或更新时自增，并且将其设置为乐观并发标识。
            //builder.Property(e => e.Version).ValueGeneratedOnAddOrUpdate().IsConcurrencyToken();

            // 配置关系一个菜单对应的对应多个按钮 关系
            builder.HasOne<SysRole>(e => e.SysRole)
                .WithMany(e => e.RolesToMenus)
                .HasForeignKey(e => e.RoleId);


        }
    }
}
