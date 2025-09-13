using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Post.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class MigrateSchemaFollowingDDDPattern : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "emotion_tags");

            migrationBuilder.DropIndex(
                name: "ix_posts_created_at_id",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "outbox_messages");

            migrationBuilder.DropColumn(
                name: "created_by_alias_id",
                table: "outbox_messages");

            migrationBuilder.DropColumn(
                name: "last_modified",
                table: "outbox_messages");

            migrationBuilder.DropColumn(
                name: "last_modified_by_alias_id",
                table: "outbox_messages");

            migrationBuilder.DropColumn(
                name: "created_by",
                table: "gifts_attach");

            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.RenameTable(
                name: "reactions",
                newName: "reactions",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "posts",
                newName: "posts",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "post_media",
                newName: "post_media",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "post_emotions",
                newName: "post_emotions",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "post_counter_deltas",
                newName: "post_counter_deltas",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "post_categories",
                newName: "post_categories",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "outbox_messages",
                newName: "outbox_messages",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "idempotency_keys",
                newName: "idempotency_keys",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "gifts_attach",
                newName: "gifts_attach",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "comments",
                newName: "comments",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "category_tags",
                newName: "category_tags",
                newSchema: "public");

            migrationBuilder.RenameColumn(
                name: "occured_on",
                schema: "public",
                table: "outbox_messages",
                newName: "occurred_on");

            migrationBuilder.RenameIndex(
                name: "ix_outbox_messages_occured_on",
                schema: "public",
                table: "outbox_messages",
                newName: "ix_outbox_messages_occurred_on");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "last_modified",
                schema: "public",
                table: "reactions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "last_modified_by_alias_id",
                schema: "public",
                table: "reactions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "last_modified",
                schema: "public",
                table: "posts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "last_modified_by_alias_id",
                schema: "public",
                table: "posts",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "created_at",
                schema: "public",
                table: "gifts_attach",
                type: "timestamp with time zone",
                nullable: true,
                defaultValueSql: "now()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "now()");

            migrationBuilder.AddColumn<string>(
                name: "created_by_alias_id",
                schema: "public",
                table: "gifts_attach",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "deleted_by_alias_id",
                schema: "public",
                table: "gifts_attach",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                schema: "public",
                table: "gifts_attach",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "last_modified",
                schema: "public",
                table: "comments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "last_modified_by_alias_id",
                schema: "public",
                table: "comments",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_posts_created_at_id",
                schema: "public",
                table: "posts",
                columns: new[] { "created_at", "id" },
                descending: new bool[0],
                filter: "((deleted_at IS NULL) AND (moderation_status = 'Approved'::text))");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_posts_created_at_id",
                schema: "public",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "last_modified",
                schema: "public",
                table: "reactions");

            migrationBuilder.DropColumn(
                name: "last_modified_by_alias_id",
                schema: "public",
                table: "reactions");

            migrationBuilder.DropColumn(
                name: "last_modified",
                schema: "public",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "last_modified_by_alias_id",
                schema: "public",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "created_by_alias_id",
                schema: "public",
                table: "gifts_attach");

            migrationBuilder.DropColumn(
                name: "deleted_by_alias_id",
                schema: "public",
                table: "gifts_attach");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                schema: "public",
                table: "gifts_attach");

            migrationBuilder.DropColumn(
                name: "last_modified",
                schema: "public",
                table: "comments");

            migrationBuilder.DropColumn(
                name: "last_modified_by_alias_id",
                schema: "public",
                table: "comments");

            migrationBuilder.RenameTable(
                name: "reactions",
                schema: "public",
                newName: "reactions");

            migrationBuilder.RenameTable(
                name: "posts",
                schema: "public",
                newName: "posts");

            migrationBuilder.RenameTable(
                name: "post_media",
                schema: "public",
                newName: "post_media");

            migrationBuilder.RenameTable(
                name: "post_emotions",
                schema: "public",
                newName: "post_emotions");

            migrationBuilder.RenameTable(
                name: "post_counter_deltas",
                schema: "public",
                newName: "post_counter_deltas");

            migrationBuilder.RenameTable(
                name: "post_categories",
                schema: "public",
                newName: "post_categories");

            migrationBuilder.RenameTable(
                name: "outbox_messages",
                schema: "public",
                newName: "outbox_messages");

            migrationBuilder.RenameTable(
                name: "idempotency_keys",
                schema: "public",
                newName: "idempotency_keys");

            migrationBuilder.RenameTable(
                name: "gifts_attach",
                schema: "public",
                newName: "gifts_attach");

            migrationBuilder.RenameTable(
                name: "comments",
                schema: "public",
                newName: "comments");

            migrationBuilder.RenameTable(
                name: "category_tags",
                schema: "public",
                newName: "category_tags");

            migrationBuilder.RenameColumn(
                name: "occurred_on",
                table: "outbox_messages",
                newName: "occured_on");

            migrationBuilder.RenameIndex(
                name: "ix_outbox_messages_occurred_on",
                table: "outbox_messages",
                newName: "ix_outbox_messages_occured_on");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "created_at",
                table: "outbox_messages",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "created_by_alias_id",
                table: "outbox_messages",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "last_modified",
                table: "outbox_messages",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "last_modified_by_alias_id",
                table: "outbox_messages",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "gifts_attach",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()",
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldDefaultValueSql: "now()");

            migrationBuilder.AddColumn<Guid>(
                name: "created_by",
                table: "gifts_attach",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "emotion_tags",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "text", nullable: false),
                    color = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, defaultValueSql: "now()"),
                    created_by_alias_id = table.Column<string>(type: "text", nullable: true),
                    digital_good_id = table.Column<Guid>(type: "uuid", nullable: true),
                    display_name = table.Column<string>(type: "text", nullable: false),
                    icon = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    last_modified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_modified_by_alias_id = table.Column<string>(type: "text", nullable: true),
                    media_id = table.Column<Guid>(type: "uuid", nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    topic = table.Column<string>(type: "text", nullable: true),
                    unicode_codepoint = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("emotion_tags_pkey", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_posts_created_at_id",
                table: "posts",
                columns: new[] { "created_at", "id" },
                descending: new bool[0],
                filter: "((deleted_at IS NULL) AND (moderation_status = 'approved'::text))");

            migrationBuilder.CreateIndex(
                name: "ix_emotion_tags_code",
                table: "emotion_tags",
                column: "code",
                unique: true);
        }
    }
}
