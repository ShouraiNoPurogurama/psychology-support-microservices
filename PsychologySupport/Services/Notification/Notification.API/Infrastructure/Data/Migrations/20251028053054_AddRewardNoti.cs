using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Notification.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRewardNoti : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "reward_id",
                schema: "public",
                table: "user_notifications",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "session_id",
                schema: "public",
                table: "user_notifications",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "reward_id",
                schema: "public",
                table: "user_notifications");

            migrationBuilder.DropColumn(
                name: "session_id",
                schema: "public",
                table: "user_notifications");
        }
    }
}
