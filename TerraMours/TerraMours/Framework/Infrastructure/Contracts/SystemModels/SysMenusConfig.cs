﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TerraMours.Framework.Infrastructure.Contracts.SystemModels
{
    public class SysMenusConfig : IEntityTypeConfiguration<SysMenus>
    {
        public void Configure(EntityTypeBuilder<SysMenus> builder)
        {
            //在 PostgreSQL 中，表名以及列名是不区分大小写的。这意味着在数据库中创建表时，无论您是使用大写、小写或混合大小写的名称，最终都会使用相同的名称来访问和操作该表。
            //设置表
            builder.ToTable("SysMenus");
            //设置表主键
            builder.HasKey(e => e.MenuId);
            //设置主键自增
            builder.Property(e => e.MenuId)
                   .UseIdentityColumn();

        }
    }
}