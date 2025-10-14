using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Alias.API.Data.Public.Migrations
{
    /// <inheritdoc />
    public partial class AddPostsCountMetric : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "posts_count",
                table: "aliases",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "posts_count",
                table: "aliases");
        }
    }
}
