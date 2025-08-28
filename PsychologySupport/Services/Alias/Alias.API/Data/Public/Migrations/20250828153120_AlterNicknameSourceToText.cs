using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Alias.API.Data.Public.Migrations
{
    /// <inheritdoc />
    public partial class AlterNicknameSourceToText : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "nickname_source",
                table: "alias_versions",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nickname_source");
            
            migrationBuilder.RenameTable(
                name: "aliases",
                schema: "public",
                newName: "aliases");

            migrationBuilder.RenameTable(
                name: "alias_versions",
                schema: "public",
                newName: "alias_versions");

            migrationBuilder.RenameTable(
                name: "alias_audits",
                schema: "public",
                newName: "alias_audits");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:citext", ",,")
                .OldAnnotation("Npgsql:Enum:public.nickname_source", "gacha,custom")
                .OldAnnotation("Npgsql:PostgresExtension:citext", ",,");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.RenameTable(
                name: "aliases",
                newName: "aliases",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "alias_versions",
                newName: "alias_versions",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "alias_audits",
                newName: "alias_audits",
                newSchema: "public");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:public.nickname_source", "gacha,custom")
                .Annotation("Npgsql:PostgresExtension:citext", ",,")
                .OldAnnotation("Npgsql:PostgresExtension:citext", ",,");

            migrationBuilder.AlterColumn<string>(
                name: "nickname_source",
                schema: "public",
                table: "alias_versions",
                type: "nickname_source",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
