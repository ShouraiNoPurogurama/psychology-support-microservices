using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Post.Infrastructure.Data.Post.Migrations
{
    /// <inheritdoc />
    public partial class AddRemainingEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "is_deleted",
                schema: "post",
                table: "post_emotions",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<double>(
                name: "confidence",
                schema: "post",
                table: "post_emotions",
                type: "double precision",
                precision: 3,
                scale: 2,
                nullable: false,
                defaultValue: 1.0,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldPrecision: 3,
                oldScale: 2);

            migrationBuilder.CreateTable(
                name: "category_tags",
                schema: "post",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "text", nullable: false),
                    display_name = table.Column<string>(type: "text", nullable: false),
                    color = table.Column<string>(type: "text", nullable: true),
                    unicode_codepoint = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_by_alias_id = table.Column<string>(type: "text", nullable: true),
                    last_modified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_modified_by_alias_id = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_category_tags", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "gift_attaches",
                schema: "post",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    target_target_type = table.Column<string>(type: "text", nullable: false),
                    target_target_id = table.Column<Guid>(type: "uuid", nullable: false),
                    info_gift_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sender_alias_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sender_alias_version_id = table.Column<Guid>(type: "uuid", nullable: true),
                    message = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    sent_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    deleted_by_alias_id = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_by_alias_id = table.Column<string>(type: "text", nullable: true),
                    last_modified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_modified_by_alias_id = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_gift_attaches", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "idempotency_keys",
                schema: "post",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    key = table.Column<Guid>(type: "uuid", nullable: false),
                    request_hash = table.Column<string>(type: "text", nullable: false),
                    response_payload = table.Column<string>(type: "text", nullable: true),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, defaultValueSql: "now()"),
                    created_by_alias_id = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("idempotency_keys_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "outbox_messages",
                schema: "post",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    occurred_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    processed_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("outbox_messages_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "post_counter_delta",
                schema: "post",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    post_id = table.Column<Guid>(type: "uuid", nullable: false),
                    counter_type = table.Column<string>(type: "text", nullable: false),
                    delta = table.Column<short>(type: "smallint", nullable: false),
                    occurred_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_processed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_post_counter_delta", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_posts_feed_mod_status",
                schema: "post",
                table: "posts",
                columns: new[] { "moderation_status", "moderated_at" });

            migrationBuilder.CreateIndex(
                name: "ix_posts_feed_vis_created",
                schema: "post",
                table: "posts",
                columns: new[] { "visibility", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_post_categories_category_tag_id",
                schema: "post",
                table: "post_categories",
                column: "category_tag_id");

            migrationBuilder.CreateIndex(
                name: "ix_comments_post_created",
                schema: "post",
                table: "comments",
                columns: new[] { "post_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ux_category_tags_code",
                schema: "post",
                table: "category_tags",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_idempotency_keys_key",
                schema: "post",
                table: "idempotency_keys",
                column: "key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_occurred_on",
                schema: "post",
                table: "outbox_messages",
                column: "occurred_on",
                filter: "(processed_on IS NULL)");

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_processed_on",
                schema: "post",
                table: "outbox_messages",
                column: "processed_on",
                filter: "(processed_on IS NOT NULL)");

            migrationBuilder.CreateIndex(
                name: "ix_pcd_pending_time",
                schema: "post",
                table: "post_counter_delta",
                column: "occurred_at",
                filter: "(is_processed = false)");

            migrationBuilder.CreateIndex(
                name: "ix_pcd_post_kind_time",
                schema: "post",
                table: "post_counter_delta",
                columns: new[] { "post_id", "counter_type", "occurred_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "category_tags",
                schema: "post");

            migrationBuilder.DropTable(
                name: "gift_attaches",
                schema: "post");

            migrationBuilder.DropTable(
                name: "idempotency_keys",
                schema: "post");

            migrationBuilder.DropTable(
                name: "outbox_messages",
                schema: "post");

            migrationBuilder.DropTable(
                name: "post_counter_delta",
                schema: "post");

            migrationBuilder.DropIndex(
                name: "ix_posts_feed_mod_status",
                schema: "post",
                table: "posts");

            migrationBuilder.DropIndex(
                name: "ix_posts_feed_vis_created",
                schema: "post",
                table: "posts");

            migrationBuilder.DropIndex(
                name: "ix_post_categories_category_tag_id",
                schema: "post",
                table: "post_categories");

            migrationBuilder.DropIndex(
                name: "ix_comments_post_created",
                schema: "post",
                table: "comments");

            migrationBuilder.AlterColumn<bool>(
                name: "is_deleted",
                schema: "post",
                table: "post_emotions",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<double>(
                name: "confidence",
                schema: "post",
                table: "post_emotions",
                type: "double precision",
                precision: 3,
                scale: 2,
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double precision",
                oldPrecision: 3,
                oldScale: 2,
                oldDefaultValue: 1.0);
        }
    }
}
