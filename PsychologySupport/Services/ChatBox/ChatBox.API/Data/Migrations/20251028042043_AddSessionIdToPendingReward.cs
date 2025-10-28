using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatBox.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSessionIdToPendingReward : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "session_id",
                schema: "public",
                table: "pending_sticker_rewards",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "session_id",
                schema: "public",
                table: "pending_sticker_rewards");
        }
    }
}
