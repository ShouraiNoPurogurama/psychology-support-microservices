using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wellness.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddModuleCategoryToModuleSection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "category",
                table: "module_sections",
                type: "VARCHAR(30)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "category",
                table: "module_sections");
        }
    }
}
