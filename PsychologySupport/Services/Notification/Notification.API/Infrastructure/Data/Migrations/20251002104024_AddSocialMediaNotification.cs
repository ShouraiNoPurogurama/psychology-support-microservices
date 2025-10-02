using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Notification.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSocialMediaNotification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "notification_preferences",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    reactions_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    comments_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    mentions_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    follows_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    moderation_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    bot_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    system_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    last_modified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_modified_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notification_preferences", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "processed_integration_events",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_type = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    received_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    last_modified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_modified_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_processed_integration_events", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user_notifications",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    recipient_alias_id = table.Column<Guid>(type: "uuid", nullable: false),
                    actor_alias_id = table.Column<Guid>(type: "uuid", nullable: true),
                    actor_display_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    is_read = table.Column<bool>(type: "boolean", nullable: false),
                    read_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    post_id = table.Column<Guid>(type: "uuid", nullable: true),
                    comment_id = table.Column<Guid>(type: "uuid", nullable: true),
                    reaction_id = table.Column<Guid>(type: "uuid", nullable: true),
                    follow_id = table.Column<Guid>(type: "uuid", nullable: true),
                    moderation_action = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    snippet = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    grouping_key = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    dedupe_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    version = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    last_modified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_modified_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_notifications", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "idx_processed_events_received",
                schema: "public",
                table: "processed_integration_events",
                column: "received_at");

            migrationBuilder.CreateIndex(
                name: "idx_user_notifications_dedupe",
                schema: "public",
                table: "user_notifications",
                column: "dedupe_hash");

            migrationBuilder.CreateIndex(
                name: "idx_user_notifications_grouping",
                schema: "public",
                table: "user_notifications",
                columns: new[] { "grouping_key", "recipient_alias_id", "is_read" });

            migrationBuilder.CreateIndex(
                name: "idx_user_notifications_recipient_created",
                schema: "public",
                table: "user_notifications",
                columns: new[] { "recipient_alias_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "idx_user_notifications_recipient_type",
                schema: "public",
                table: "user_notifications",
                columns: new[] { "recipient_alias_id", "type", "created_at" });

            migrationBuilder.CreateIndex(
                name: "idx_user_notifications_recipient_unread",
                schema: "public",
                table: "user_notifications",
                columns: new[] { "recipient_alias_id", "is_read", "created_at" },
                filter: "is_read = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "notification_preferences",
                schema: "public");

            migrationBuilder.DropTable(
                name: "processed_integration_events",
                schema: "public");

            migrationBuilder.DropTable(
                name: "user_notifications",
                schema: "public");
        }
    }
}
