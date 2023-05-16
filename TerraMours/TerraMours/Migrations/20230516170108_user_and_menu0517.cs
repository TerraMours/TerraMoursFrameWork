using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TerraMours.Migrations
{
    /// <inheritdoc />
    public partial class user_and_menu0517 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSuperAdmin",
                table: "SysUser",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ExternalUrl",
                table: "SysMenus",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsHome",
                table: "SysMenus",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsShow",
                table: "SysMenus",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSuperAdmin",
                table: "SysUser");

            migrationBuilder.DropColumn(
                name: "ExternalUrl",
                table: "SysMenus");

            migrationBuilder.DropColumn(
                name: "IsHome",
                table: "SysMenus");

            migrationBuilder.DropColumn(
                name: "IsShow",
                table: "SysMenus");
        }
    }
}
