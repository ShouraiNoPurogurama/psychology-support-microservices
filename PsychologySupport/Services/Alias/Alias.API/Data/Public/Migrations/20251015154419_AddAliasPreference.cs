using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Alias.API.Data.Public.Migrations
{
    /// <inheritdoc />
    public partial class AddAliasPreference : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "preference_language",
                table: "aliases",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "VI");

            migrationBuilder.AddColumn<bool>(
                name: "preference_notifications_enabled",
                table: "aliases",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "preference_theme",
                table: "aliases",
                type: "character varying(15)",
                maxLength: 15,
                nullable: false,
                defaultValue: "Light");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "preference_language",
                table: "aliases");

            migrationBuilder.DropColumn(
                name: "preference_notifications_enabled",
                table: "aliases");

            migrationBuilder.DropColumn(
                name: "preference_theme",
                table: "aliases");
        }
    }
}
