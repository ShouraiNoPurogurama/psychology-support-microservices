using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Alias.API.Data.Public.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUniqueConstraintOnSearchKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_alias_versions_search_key",
                table: "alias_versions");

            migrationBuilder.CreateIndex(
                name: "ix_alias_versions_search_key",
                table: "alias_versions",
                column: "search_key",
                filter: "(valid_to IS NULL)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_alias_versions_search_key",
                table: "alias_versions");

            migrationBuilder.CreateIndex(
                name: "ix_alias_versions_search_key",
                table: "alias_versions",
                column: "search_key",
                unique: true,
                filter: "(valid_to IS NULL)");
        }
    }
}
