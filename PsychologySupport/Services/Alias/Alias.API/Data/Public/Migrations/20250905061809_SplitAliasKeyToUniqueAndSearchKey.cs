using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Alias.API.Data.Public.Migrations
{
    /// <inheritdoc />
    public partial class SplitAliasKeyToUniqueAndSearchKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "alias_key",
                table: "alias_versions",
                newName: "unique_key");

            migrationBuilder.RenameIndex(
                name: "ix_alias_versions_alias_key",
                table: "alias_versions",
                newName: "ix_alias_versions_unique_key");

            migrationBuilder.AddColumn<string>(
                name: "search_key",
                table: "alias_versions",
                type: "citext",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "ix_alias_versions_search_key",
                table: "alias_versions",
                column: "search_key",
                unique: true,
                filter: "(valid_to IS NULL)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_alias_versions_search_key",
                table: "alias_versions");

            migrationBuilder.DropColumn(
                name: "search_key",
                table: "alias_versions");

            migrationBuilder.RenameColumn(
                name: "unique_key",
                table: "alias_versions",
                newName: "alias_key");

            migrationBuilder.RenameIndex(
                name: "ix_alias_versions_unique_key",
                table: "alias_versions",
                newName: "ix_alias_versions_alias_key");
        }
    }
}
