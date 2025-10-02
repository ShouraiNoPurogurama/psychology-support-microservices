using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wellness.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateColumnName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "status",
                table: "challenges",
                newName: "challenge_type");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "activities",
                newName: "activity_type");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "challenge_type",
                table: "challenges",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "activity_type",
                table: "activities",
                newName: "status");
        }
    }
}
