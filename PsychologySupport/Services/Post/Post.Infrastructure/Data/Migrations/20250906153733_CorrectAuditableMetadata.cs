using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Post.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class CorrectAuditableMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "created_by",
                table: "reactions");

            migrationBuilder.DropColumn(
                name: "created_by",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "last_modified",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "last_modified_by",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "created_by",
                table: "post_media");

            migrationBuilder.DropColumn(
                name: "created_by",
                table: "comments");

            migrationBuilder.DropColumn(
                name: "last_modified",
                table: "comments");

            migrationBuilder.DropColumn(
                name: "last_modified_by",
                table: "comments");

            migrationBuilder.RenameColumn(
                name: "last_modified_by",
                table: "outbox_messages",
                newName: "last_modified_by_alias_id");

            migrationBuilder.RenameColumn(
                name: "created_by",
                table: "outbox_messages",
                newName: "created_by_alias_id");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "created_at",
                table: "reactions",
                type: "timestamp with time zone",
                nullable: true,
                defaultValueSql: "now()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "now()");

            migrationBuilder.AddColumn<string>(
                name: "created_by_alias_id",
                table: "reactions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "deleted_by_alias_id",
                table: "reactions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "reactions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "created_at",
                table: "posts",
                type: "timestamp with time zone",
                nullable: true,
                defaultValueSql: "now()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "now()");

            migrationBuilder.AddColumn<string>(
                name: "created_by_alias_id",
                table: "posts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "deleted_by_alias_id",
                table: "posts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "posts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<int>(
                name: "position",
                table: "post_media",
                type: "integer",
                nullable: true,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "created_at",
                table: "post_media",
                type: "timestamp with time zone",
                nullable: true,
                defaultValueSql: "now()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "now()");

            migrationBuilder.AddColumn<string>(
                name: "created_by_alias_id",
                table: "post_media",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "deleted_by_alias_id",
                table: "post_media",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "post_media",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "created_by_alias_id",
                table: "post_emotions",
                type: "text",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "created_at",
                table: "post_emotions",
                type: "timestamp with time zone",
                nullable: true,
                defaultValueSql: "now()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "now()");

            migrationBuilder.AddColumn<string>(
                name: "deleted_by_alias_id",
                table: "post_emotions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "post_emotions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "created_by_alias_id",
                table: "post_categories",
                type: "text",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "created_at",
                table: "post_categories",
                type: "timestamp with time zone",
                nullable: true,
                defaultValueSql: "now()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "now()");

            migrationBuilder.AddColumn<string>(
                name: "deleted_by_alias_id",
                table: "post_categories",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "post_categories",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "created_at",
                table: "idempotency_keys",
                type: "timestamp with time zone",
                nullable: true,
                defaultValueSql: "now()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "now()");

            migrationBuilder.AddColumn<string>(
                name: "created_by_alias_id",
                table: "idempotency_keys",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "created_at",
                table: "emotion_tags",
                type: "timestamp with time zone",
                nullable: true,
                defaultValueSql: "now()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "now()");

            migrationBuilder.AddColumn<string>(
                name: "created_by_alias_id",
                table: "emotion_tags",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "last_modified_by_alias_id",
                table: "emotion_tags",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "created_at",
                table: "comments",
                type: "timestamp with time zone",
                nullable: true,
                defaultValueSql: "now()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "now()");

            migrationBuilder.AddColumn<string>(
                name: "created_by_alias_id",
                table: "comments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "deleted_by_alias_id",
                table: "comments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "comments",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "created_at",
                table: "category_tags",
                type: "timestamp with time zone",
                nullable: true,
                defaultValueSql: "now()",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "now()");

            migrationBuilder.AddColumn<string>(
                name: "created_by_alias_id",
                table: "category_tags",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "last_modified_by_alias_id",
                table: "category_tags",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "created_by_alias_id",
                table: "reactions");

            migrationBuilder.DropColumn(
                name: "deleted_by_alias_id",
                table: "reactions");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "reactions");

            migrationBuilder.DropColumn(
                name: "created_by_alias_id",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "deleted_by_alias_id",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "posts");

            migrationBuilder.DropColumn(
                name: "created_by_alias_id",
                table: "post_media");

            migrationBuilder.DropColumn(
                name: "deleted_by_alias_id",
                table: "post_media");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "post_media");

            migrationBuilder.DropColumn(
                name: "deleted_by_alias_id",
                table: "post_emotions");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "post_emotions");

            migrationBuilder.DropColumn(
                name: "deleted_by_alias_id",
                table: "post_categories");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "post_categories");

            migrationBuilder.DropColumn(
                name: "created_by_alias_id",
                table: "idempotency_keys");

            migrationBuilder.DropColumn(
                name: "created_by_alias_id",
                table: "emotion_tags");

            migrationBuilder.DropColumn(
                name: "last_modified_by_alias_id",
                table: "emotion_tags");

            migrationBuilder.DropColumn(
                name: "created_by_alias_id",
                table: "comments");

            migrationBuilder.DropColumn(
                name: "deleted_by_alias_id",
                table: "comments");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "comments");

            migrationBuilder.DropColumn(
                name: "created_by_alias_id",
                table: "category_tags");

            migrationBuilder.DropColumn(
                name: "last_modified_by_alias_id",
                table: "category_tags");

            migrationBuilder.RenameColumn(
                name: "last_modified_by_alias_id",
                table: "outbox_messages",
                newName: "last_modified_by");

            migrationBuilder.RenameColumn(
                name: "created_by_alias_id",
                table: "outbox_messages",
                newName: "created_by");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "reactions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()",
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldDefaultValueSql: "now()");

            migrationBuilder.AddColumn<Guid>(
                name: "created_by",
                table: "reactions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "posts",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()",
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldDefaultValueSql: "now()");

            migrationBuilder.AddColumn<Guid>(
                name: "created_by",
                table: "posts",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "last_modified",
                table: "posts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "last_modified_by",
                table: "posts",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "position",
                table: "post_media",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true,
                oldDefaultValue: 0);

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "post_media",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()",
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldDefaultValueSql: "now()");

            migrationBuilder.AddColumn<Guid>(
                name: "created_by",
                table: "post_media",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<Guid>(
                name: "created_by_alias_id",
                table: "post_emotions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "post_emotions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()",
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldDefaultValueSql: "now()");

            migrationBuilder.AlterColumn<Guid>(
                name: "created_by_alias_id",
                table: "post_categories",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "post_categories",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()",
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldDefaultValueSql: "now()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "idempotency_keys",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()",
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldDefaultValueSql: "now()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "emotion_tags",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()",
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldDefaultValueSql: "now()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "comments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()",
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldDefaultValueSql: "now()");

            migrationBuilder.AddColumn<Guid>(
                name: "created_by",
                table: "comments",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<DateTime>(
                name: "last_modified",
                table: "comments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "last_modified_by",
                table: "comments",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "created_at",
                table: "category_tags",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "now()",
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldDefaultValueSql: "now()");
        }
    }
}
