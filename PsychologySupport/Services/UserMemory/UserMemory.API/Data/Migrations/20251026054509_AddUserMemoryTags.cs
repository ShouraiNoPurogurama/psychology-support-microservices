using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserMemory.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserMemoryTags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_user_memories_tags",
                table: "user_memories");

            migrationBuilder.DropColumn(
                name: "tags",
                table: "user_memories");

            migrationBuilder.CreateTable(
                name: "memory_tags",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    category = table.Column<string>(type: "text", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_memory_tags", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user_memory_tags",
                columns: table => new
                {
                    user_memory_id = table.Column<Guid>(type: "uuid", nullable: false),
                    memory_tag_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_memory_tags", x => new { x.user_memory_id, x.memory_tag_id });
                    table.ForeignKey(
                        name: "fk_user_memory_tags_memory_tags_memory_tag_id",
                        column: x => x.memory_tag_id,
                        principalTable: "memory_tags",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_user_memory_tags_user_memories_user_memory_id",
                        column: x => x.user_memory_id,
                        principalTable: "user_memories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_memory_tags_code",
                table: "memory_tags",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_memory_tags_memory_tag_id",
                table: "user_memory_tags",
                column: "memory_tag_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_memory_tags_user_memory_id",
                table: "user_memory_tags",
                column: "user_memory_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_memory_tags");

            migrationBuilder.DropTable(
                name: "memory_tags");

            migrationBuilder.AddColumn<List<string>>(
                name: "tags",
                table: "user_memories",
                type: "text[]",
                nullable: false);

            migrationBuilder.CreateIndex(
                name: "ix_user_memories_tags",
                table: "user_memories",
                column: "tags")
                .Annotation("Npgsql:IndexMethod", "gin");
        }
    }
}
