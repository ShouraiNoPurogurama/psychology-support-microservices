using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Notification.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGiftIdSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "gift_id",
                schema: "public",
                table: "user_notifications",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "gift_id",
                schema: "public",
                table: "user_notifications");
        }
    }
}
