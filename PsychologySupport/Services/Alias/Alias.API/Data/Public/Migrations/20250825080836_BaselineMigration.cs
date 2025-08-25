using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Alias.API.Data.Public.Migrations
{
    /// <inheritdoc />
    public partial class BaselineMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:public.nickname_source", "gacha,custom")
                .Annotation("Npgsql:PostgresExtension:citext", ",,");

            migrationBuilder.CreateTable(
                name: "alias_audits",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    alias_id = table.Column<Guid>(type: "uuid", nullable: false),
                    action = table.Column<string>(type: "text", nullable: false),
                    details = table.Column<string>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("alias_audits_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "alias_versions",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    alias_id = table.Column<Guid>(type: "uuid", nullable: false),
                    alias_label = table.Column<string>(type: "text", nullable: false),
                    alias_key = table.Column<string>(type: "citext", nullable: false),
                    nickname_source = table.Column<string>(type: "public.nickname_source", nullable: false),
                    valid_from = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    valid_to = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("alias_versions_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "aliases",
                schema: "public",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    current_version_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    last_modified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_modified_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("aliases_pkey", x => x.id);
                    table.ForeignKey(
                        name: "aliases_current_version_id_fkey",
                        column: x => x.current_version_id,
                        principalSchema: "public",
                        principalTable: "alias_versions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "ix_alias_audits_alias_id",
                schema: "public",
                table: "alias_audits",
                column: "alias_id");

            migrationBuilder.CreateIndex(
                name: "ix_alias_versions_alias_id",
                schema: "public",
                table: "alias_versions",
                column: "alias_id");

            migrationBuilder.CreateIndex(
                name: "ix_alias_versions_alias_key",
                schema: "public",
                table: "alias_versions",
                column: "alias_key",
                unique: true,
                filter: "(valid_to IS NULL)");

            migrationBuilder.CreateIndex(
                name: "ix_aliases_current_version_id",
                schema: "public",
                table: "aliases",
                column: "current_version_id");

            migrationBuilder.AddForeignKey(
                name: "alias_versions_alias_id_fkey",
                schema: "public",
                table: "alias_versions",
                column: "alias_id",
                principalSchema: "public",
                principalTable: "aliases",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "alias_versions_alias_id_fkey",
                schema: "public",
                table: "alias_versions");

            migrationBuilder.DropTable(
                name: "alias_audits",
                schema: "public");

            migrationBuilder.DropTable(
                name: "aliases",
                schema: "public");

            migrationBuilder.DropTable(
                name: "alias_versions",
                schema: "public");
        }
    }
}
