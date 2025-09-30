using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wellness.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddArticleSource : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "source_description",
                table: "section_articles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "source_name",
                table: "section_articles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "source_url",
                table: "section_articles",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "source_description",
                table: "section_articles");

            migrationBuilder.DropColumn(
                name: "source_name",
                table: "section_articles");

            migrationBuilder.DropColumn(
                name: "source_url",
                table: "section_articles");
        }
    }
}
