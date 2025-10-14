using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Alias.API.Data.Public.Migrations
{
    /// <inheritdoc />
    public partial class AddReactionMetricsToAliasMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "comments_count",
                table: "aliases",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "reaction_given_count",
                table: "aliases",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "reaction_received_count",
                table: "aliases",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "shares_count",
                table: "aliases",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "comments_count",
                table: "aliases");

            migrationBuilder.DropColumn(
                name: "reaction_given_count",
                table: "aliases");

            migrationBuilder.DropColumn(
                name: "reaction_received_count",
                table: "aliases");

            migrationBuilder.DropColumn(
                name: "shares_count",
                table: "aliases");
        }
    }
}
