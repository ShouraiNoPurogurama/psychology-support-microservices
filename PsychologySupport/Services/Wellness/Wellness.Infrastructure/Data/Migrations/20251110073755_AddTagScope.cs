using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wellness.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTagScope : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "scope",
                table: "section_articles",
                type: "VARCHAR(20)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "scope",
                table: "challenges",
                type: "VARCHAR(20)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "scope",
                table: "section_articles");

            migrationBuilder.DropColumn(
                name: "scope",
                table: "challenges");
        }
    }
}
