using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Post.Infrastructure.Data.Query.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "query");

            migrationBuilder.CreateTable(
                name: "emotion_tag_replicas",
                schema: "query",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "text", nullable: false),
                    display_name = table.Column<string>(type: "text", nullable: false),
                    media_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    last_synced_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_emotion_tag_replicas", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user_owned_tag_replicas",
                schema: "query",
                columns: table => new
                {
                    subject_ref = table.Column<Guid>(type: "uuid", nullable: false),
                    emotion_tag_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_owned_tag_replicas", x => new { x.subject_ref, x.emotion_tag_id });
                });

            migrationBuilder.CreateIndex(
                name: "ix_emotion_tag_replicas_code",
                schema: "query",
                table: "emotion_tag_replicas",
                column: "code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "emotion_tag_replicas",
                schema: "query");

            migrationBuilder.DropTable(
                name: "user_owned_tag_replicas",
                schema: "query");
        }
    }
}
