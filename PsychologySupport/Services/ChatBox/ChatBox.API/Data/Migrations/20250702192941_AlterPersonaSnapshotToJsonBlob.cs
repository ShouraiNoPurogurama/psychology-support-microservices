using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatBox.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AlterPersonaSnapshotToJsonBlob : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                                     UPDATE "public"."AIChatSessions"
                                     SET "PersonaSnapshot" = NULL
                                     WHERE "PersonaSnapshot" = '';
                                 """);
            //Xóa default value cũ (nếu có)
            migrationBuilder.Sql("""
                                     ALTER TABLE "public"."AIChatSessions"
                                     ALTER COLUMN "PersonaSnapshot" DROP DEFAULT;
                                 """);
            migrationBuilder.Sql("""
                                     ALTER TABLE "public"."AIChatSessions"
                                     ALTER COLUMN "PersonaSnapshot" TYPE jsonb
                                     USING "PersonaSnapshot"::jsonb;
                                 """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PersonaSnapshot",
                schema: "public",
                table: "AIChatSessions",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "jsonb",
                oldNullable: true);
        }
    }
}
