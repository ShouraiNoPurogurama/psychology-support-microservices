using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Alias.API.Data.Public.Migrations
{
    /// <inheritdoc />
    public partial class AdjustActionMapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "deleted_by_alias_id",
                table: "aliases");

            migrationBuilder.AlterColumn<string>(
                name: "action",
                table: "alias_audits",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "deleted_by_alias_id",
                table: "aliases",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "action",
                table: "alias_audits",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
            
            
        }
    }
}
