using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auth.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixPendingUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "pk_pending_verification_user",
                table: "pending_verification_user");

            migrationBuilder.RenameTable(
                name: "pending_verification_user",
                newName: "pending_verification_users");

            migrationBuilder.AddPrimaryKey(
                name: "pk_pending_verification_users",
                table: "pending_verification_users",
                column: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "pk_pending_verification_users",
                table: "pending_verification_users");

            migrationBuilder.RenameTable(
                name: "pending_verification_users",
                newName: "pending_verification_user");

            migrationBuilder.AddPrimaryKey(
                name: "pk_pending_verification_user",
                table: "pending_verification_user",
                column: "id");
        }
    }
}
