using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatBox.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPendingStickerRewards : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "pending_sticker_rewards",
                schema: "public",
                columns: table => new
                {
                    reward_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sticker_url = table.Column<string>(type: "text", nullable: false),
                    prompt_filter = table.Column<string>(type: "text", nullable: false),
                    is_claimed = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    claimed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pending_sticker_rewards", x => x.reward_id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "pending_sticker_rewards",
                schema: "public");
        }
    }
}
