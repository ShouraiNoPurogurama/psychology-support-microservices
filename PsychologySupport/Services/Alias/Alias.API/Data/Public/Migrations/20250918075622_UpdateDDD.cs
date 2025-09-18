using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Alias.API.Data.Public.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDDD : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "alias_versions_alias_id_fkey",
                table: "alias_versions");

            migrationBuilder.DropForeignKey(
                name: "aliases_current_version_id_fkey",
                table: "aliases");

            migrationBuilder.DropIndex(
                name: "ix_aliases_current_version_id",
                table: "aliases");

            migrationBuilder.RenameColumn(
                name: "alias_visibility",
                table: "aliases",
                newName: "visibility");

            migrationBuilder.RenameColumn(
                name: "alias_label",
                table: "alias_versions",
                newName: "display_name");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "created_at",
                table: "aliases",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "deleted_at",
                table: "aliases",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "deleted_by",
                table: "aliases",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "deleted_by_alias_id",
                table: "aliases",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "aliases",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_system_generated",
                table: "aliases",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "last_active_at",
                table: "aliases",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "metadata_created_at",
                table: "aliases",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "search_key",
                table: "aliases",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "status",
                table: "aliases",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "suspended_at",
                table: "aliases",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "suspension_reason",
                table: "aliases",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "unique_key",
                table: "aliases",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "value",
                table: "aliases",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "version_count",
                table: "aliases",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "created_at",
                table: "alias_versions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "created_at",
                table: "alias_audits",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "action",
                table: "alias_audits",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddForeignKey(
                name: "fk_alias_audits_aliases_alias_id",
                table: "alias_audits",
                column: "alias_id",
                principalTable: "aliases",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_alias_versions_aliases_alias_id",
                table: "alias_versions",
                column: "alias_id",
                principalTable: "aliases",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_alias_audits_aliases_alias_id",
                table: "alias_audits");

            migrationBuilder.DropForeignKey(
                name: "fk_alias_versions_aliases_alias_id",
                table: "alias_versions");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "aliases");

            migrationBuilder.DropColumn(
                name: "deleted_by",
                table: "aliases");

            migrationBuilder.DropColumn(
                name: "deleted_by_alias_id",
                table: "aliases");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "aliases");

            migrationBuilder.DropColumn(
                name: "is_system_generated",
                table: "aliases");

            migrationBuilder.DropColumn(
                name: "last_active_at",
                table: "aliases");

            migrationBuilder.DropColumn(
                name: "metadata_created_at",
                table: "aliases");

            migrationBuilder.DropColumn(
                name: "search_key",
                table: "aliases");

            migrationBuilder.DropColumn(
                name: "status",
                table: "aliases");

            migrationBuilder.DropColumn(
                name: "suspended_at",
                table: "aliases");

            migrationBuilder.DropColumn(
                name: "suspension_reason",
                table: "aliases");

            migrationBuilder.DropColumn(
                name: "unique_key",
                table: "aliases");

            migrationBuilder.DropColumn(
                name: "value",
                table: "aliases");

            migrationBuilder.DropColumn(
                name: "version_count",
                table: "aliases");

            migrationBuilder.RenameColumn(
                name: "visibility",
                table: "aliases",
                newName: "alias_visibility");

            migrationBuilder.RenameColumn(
                name: "display_name",
                table: "alias_versions",
                newName: "alias_label");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "created_at",
                table: "aliases",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "created_at",
                table: "alias_versions",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "created_at",
                table: "alias_audits",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<string>(
                name: "action",
                table: "alias_audits",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.CreateIndex(
                name: "ix_aliases_current_version_id",
                table: "aliases",
                column: "current_version_id");

            migrationBuilder.AddForeignKey(
                name: "alias_versions_alias_id_fkey",
                table: "alias_versions",
                column: "alias_id",
                principalTable: "aliases",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "aliases_current_version_id_fkey",
                table: "aliases",
                column: "current_version_id",
                principalTable: "alias_versions",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
