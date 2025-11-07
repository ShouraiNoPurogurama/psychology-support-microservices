using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wellness.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddImprovementTagToChallenge : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "improvement_tag",
                table: "challenges",
                type: "VARCHAR(30)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "improvement_tag",
                table: "challenges");
        }
    }
}
