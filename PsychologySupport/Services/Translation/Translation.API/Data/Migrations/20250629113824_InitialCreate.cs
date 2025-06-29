using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Translation.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.CreateTable(
                name: "TranslatableFields",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TableName = table.Column<string>(type: "text", nullable: false),
                    FieldName = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TranslatableFields", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Translations",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TextKey = table.Column<string>(type: "text", nullable: false),
                    Lang = table.Column<string>(type: "text", nullable: false),
                    TranslatedValue = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsStable = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Translations", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "idx_TranslatableFields_Table_Field",
                schema: "public",
                table: "TranslatableFields",
                columns: new[] { "TableName", "FieldName" });

            migrationBuilder.CreateIndex(
                name: "uq_Translations_TextKey_Lang",
                schema: "public",
                table: "Translations",
                columns: new[] { "TextKey", "Lang" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TranslatableFields",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Translations",
                schema: "public");
        }
    }
}
