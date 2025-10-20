using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Post.Infrastructure.Data.Query.Migrations
{
    /// <inheritdoc />
    public partial class AddGiftReplicas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "gift_replicas",
                schema: "query",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    display_name = table.Column<string>(type: "text", nullable: false),
                    media_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    last_synced_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_gift_replicas", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user_owned_gift_replicas",
                schema: "query",
                columns: table => new
                {
                    alias_id = table.Column<Guid>(type: "uuid", nullable: false),
                    gift_id = table.Column<Guid>(type: "uuid", nullable: false),
                    last_synced_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    valid_from = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    valid_to = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_owned_gift_replicas", x => new { x.alias_id, x.gift_id });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "gift_replicas",
                schema: "query");

            migrationBuilder.DropTable(
                name: "user_owned_gift_replicas",
                schema: "query");
        }
    }
}
