using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TerraMours_Gpt.Migrations
{
    /// <inheritdoc />
    public partial class update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImagUrl",
                table: "ImageRecord",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PranslatePrompt",
                table: "ImageRecord",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Size",
                table: "ImageRecord",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImagUrl",
                table: "ImageRecord");

            migrationBuilder.DropColumn(
                name: "PranslatePrompt",
                table: "ImageRecord");

            migrationBuilder.DropColumn(
                name: "Size",
                table: "ImageRecord");
        }
    }
}
