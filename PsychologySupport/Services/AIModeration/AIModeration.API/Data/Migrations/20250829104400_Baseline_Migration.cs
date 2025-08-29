using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIModeration.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class Baseline_Migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:pgcrypto", ",,");

            migrationBuilder.CreateTable(
                name: "moderation_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    target_type = table.Column<string>(type: "text", nullable: false),
                    target_id = table.Column<Guid>(type: "uuid", nullable: false),
                    content_hash = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false, defaultValueSql: "'pending'::text"),
                    policy_version = table.Column<string>(type: "text", nullable: false, defaultValueSql: "'v1'::text"),
                    scores = table.Column<string>(type: "jsonb", nullable: true),
                    reasons = table.Column<List<string>>(type: "text[]", nullable: true),
                    decided_by = table.Column<string>(type: "text", nullable: true),
                    decided_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_modified_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("moderation_items_pkey", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "moderation_audits",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    @event = table.Column<string>(name: "event", type: "text", nullable: false),
                    data = table.Column<string>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("moderation_audits_pkey", x => x.id);
                    table.ForeignKey(
                        name: "moderation_audits_item_id_fkey",
                        column: x => x.item_id,
                        principalTable: "moderation_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_moderation_audits_item_id_created_at",
                table: "moderation_audits",
                columns: new[] { "item_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_moderation_items_content_hash",
                table: "moderation_items",
                column: "content_hash");

            migrationBuilder.CreateIndex(
                name: "ix_moderation_items_status_last_modified",
                table: "moderation_items",
                columns: new[] { "status", "last_modified" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "ix_moderation_items_target_type_target_id",
                table: "moderation_items",
                columns: new[] { "target_type", "target_id" });

            migrationBuilder.CreateIndex(
                name: "ix_moderation_items_target_type_target_id_content_hash",
                table: "moderation_items",
                columns: new[] { "target_type", "target_id", "content_hash" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "moderation_audits");

            migrationBuilder.DropTable(
                name: "moderation_items");
        }
    }
}
