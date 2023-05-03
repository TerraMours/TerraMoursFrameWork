using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TerraMours.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SysMenus",
                columns: table => new
                {
                    MenuId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ParentId = table.Column<long>(type: "bigint", nullable: false),
                    HasChildren = table.Column<bool>(type: "boolean", nullable: false),
                    MenuName = table.Column<string>(type: "text", nullable: false),
                    MenuUrl = table.Column<string>(type: "text", nullable: false),
                    Icon = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IconUrl = table.Column<string>(type: "text", nullable: true),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                    Enable = table.Column<bool>(type: "boolean", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreateID = table.Column<int>(type: "integer", nullable: true),
                    CreatorName = table.Column<string>(type: "text", nullable: true),
                    ModifyID = table.Column<int>(type: "integer", nullable: true),
                    ModifierName = table.Column<string>(type: "text", nullable: true),
                    ModifyDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Remark = table.Column<string>(type: "text", nullable: true),
                    OrderNo = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SysMenus", x => x.MenuId);
                });

            migrationBuilder.CreateTable(
                name: "SysRole",
                columns: table => new
                {
                    RoleId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleName = table.Column<string>(type: "text", nullable: false),
                    ParentId = table.Column<long>(type: "bigint", nullable: true),
                    HasChildren = table.Column<bool>(type: "boolean", nullable: false),
                    DeptId = table.Column<long>(type: "bigint", nullable: true),
                    DeptName = table.Column<string>(type: "text", nullable: true),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                    Enable = table.Column<bool>(type: "boolean", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreateID = table.Column<int>(type: "integer", nullable: true),
                    CreatorName = table.Column<string>(type: "text", nullable: true),
                    ModifyID = table.Column<int>(type: "integer", nullable: true),
                    ModifierName = table.Column<string>(type: "text", nullable: true),
                    ModifyDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Remark = table.Column<string>(type: "text", nullable: true),
                    OrderNo = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SysRole", x => x.RoleId);
                });

            migrationBuilder.CreateTable(
                name: "SysUser",
                columns: table => new
                {
                    UserId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserName = table.Column<string>(type: "text", nullable: false),
                    UserEmail = table.Column<string>(type: "text", nullable: true),
                    UserPhoneNum = table.Column<string>(type: "text", nullable: true),
                    ShortPhone = table.Column<string>(type: "text", nullable: true),
                    Address = table.Column<string>(type: "text", nullable: true),
                    UserPassword = table.Column<string>(type: "text", nullable: false),
                    IsRegregisterPhone = table.Column<bool>(type: "boolean", nullable: false),
                    UserTrueName = table.Column<string>(type: "text", nullable: true),
                    Gender = table.Column<string>(type: "text", nullable: true),
                    HeadImageUrl = table.Column<string>(type: "text", nullable: true),
                    DeptId = table.Column<string>(type: "text", nullable: true),
                    DeptName = table.Column<string>(type: "text", nullable: true),
                    RoleId = table.Column<string>(type: "text", nullable: false),
                    RoleName = table.Column<string>(type: "text", nullable: false),
                    Token = table.Column<string>(type: "text", nullable: false),
                    LoginFailCount = table.Column<int>(type: "integer", nullable: false),
                    EnableLogin = table.Column<bool>(type: "boolean", nullable: false),
                    ExpireTime = table.Column<bool>(type: "boolean", nullable: true),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                    Enable = table.Column<bool>(type: "boolean", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreateID = table.Column<int>(type: "integer", nullable: true),
                    CreatorName = table.Column<string>(type: "text", nullable: true),
                    ModifyID = table.Column<int>(type: "integer", nullable: true),
                    ModifierName = table.Column<string>(type: "text", nullable: true),
                    ModifyDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Remark = table.Column<string>(type: "text", nullable: true),
                    OrderNo = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SysUser", x => x.UserId);
                });

            migrationBuilder.CreateTable(
                name: "SysRolesToMenu",
                columns: table => new
                {
                    RolesToMenuId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<long>(type: "bigint", nullable: true),
                    UserId = table.Column<long>(type: "bigint", nullable: true),
                    MenuId = table.Column<long>(type: "bigint", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                    Enable = table.Column<bool>(type: "boolean", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreateID = table.Column<int>(type: "integer", nullable: true),
                    CreatorName = table.Column<string>(type: "text", nullable: true),
                    ModifyID = table.Column<int>(type: "integer", nullable: true),
                    ModifierName = table.Column<string>(type: "text", nullable: true),
                    ModifyDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Remark = table.Column<string>(type: "text", nullable: true),
                    OrderNo = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SysRolesToMenu", x => x.RolesToMenuId);
                    table.ForeignKey(
                        name: "FK_SysRolesToMenu_SysRole_RoleId",
                        column: x => x.RoleId,
                        principalTable: "SysRole",
                        principalColumn: "RoleId");
                });

            migrationBuilder.CreateTable(
                name: "SysMenuButtons",
                columns: table => new
                {
                    MenuButtonId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MenuId = table.Column<long>(type: "bigint", nullable: false),
                    ButtonShowName = table.Column<string>(type: "text", nullable: false),
                    ButtonEnName = table.Column<string>(type: "text", nullable: false),
                    RolesToMenuId = table.Column<long>(type: "bigint", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                    Enable = table.Column<bool>(type: "boolean", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreateID = table.Column<int>(type: "integer", nullable: true),
                    CreatorName = table.Column<string>(type: "text", nullable: true),
                    ModifyID = table.Column<int>(type: "integer", nullable: true),
                    ModifierName = table.Column<string>(type: "text", nullable: true),
                    ModifyDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Remark = table.Column<string>(type: "text", nullable: true),
                    OrderNo = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SysMenuButtons", x => x.MenuButtonId);
                    table.ForeignKey(
                        name: "FK_SysMenuButtons_SysMenus_MenuId",
                        column: x => x.MenuId,
                        principalTable: "SysMenus",
                        principalColumn: "MenuId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SysMenuButtons_SysRolesToMenu_MenuId",
                        column: x => x.MenuId,
                        principalTable: "SysRolesToMenu",
                        principalColumn: "RolesToMenuId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SysMenuButtons_MenuId",
                table: "SysMenuButtons",
                column: "MenuId");

            migrationBuilder.CreateIndex(
                name: "IX_SysRolesToMenu_RoleId",
                table: "SysRolesToMenu",
                column: "RoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SysMenuButtons");

            migrationBuilder.DropTable(
                name: "SysUser");

            migrationBuilder.DropTable(
                name: "SysMenus");

            migrationBuilder.DropTable(
                name: "SysRolesToMenu");

            migrationBuilder.DropTable(
                name: "SysRole");
        }
    }
}
