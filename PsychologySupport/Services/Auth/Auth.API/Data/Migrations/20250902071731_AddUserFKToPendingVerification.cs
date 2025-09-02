using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auth.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserFKToPendingVerification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "user_id",
                table: "pending_verification_users",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "ix_pending_verification_users_user_id",
                table: "pending_verification_users",
                column: "user_id",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_pending_verification_users_user_user_id",
                table: "pending_verification_users",
                column: "user_id",
                principalTable: "AspNetUsers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_pending_verification_users_user_user_id",
                table: "pending_verification_users");

            migrationBuilder.DropIndex(
                name: "ix_pending_verification_users_user_id",
                table: "pending_verification_users");

            migrationBuilder.DropColumn(
                name: "user_id",
                table: "pending_verification_users");
        }
    }
}
