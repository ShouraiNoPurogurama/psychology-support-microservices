using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Translation.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AdjustDbNamingConventionToSnakeCase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Translations",
                schema: "public",
                table: "Translations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TranslatableFields",
                schema: "public",
                table: "TranslatableFields");

            migrationBuilder.RenameColumn(
                name: "Lang",
                schema: "public",
                table: "Translations",
                newName: "lang");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "public",
                table: "Translations",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "TranslatedValue",
                schema: "public",
                table: "Translations",
                newName: "translated_value");

            migrationBuilder.RenameColumn(
                name: "TextKey",
                schema: "public",
                table: "Translations",
                newName: "text_key");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                schema: "public",
                table: "Translations",
                newName: "last_modified");

            migrationBuilder.RenameColumn(
                name: "IsStable",
                schema: "public",
                table: "Translations",
                newName: "is_stable");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                schema: "public",
                table: "Translations",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "public",
                table: "TranslatableFields",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "TableName",
                schema: "public",
                table: "TranslatableFields",
                newName: "table_name");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                schema: "public",
                table: "TranslatableFields",
                newName: "is_active");

            migrationBuilder.RenameColumn(
                name: "FieldName",
                schema: "public",
                table: "TranslatableFields",
                newName: "field_name");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                schema: "public",
                table: "TranslatableFields",
                newName: "created_at");

            migrationBuilder.AddPrimaryKey(
                name: "pk_translations",
                schema: "public",
                table: "Translations",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_translatable_fields",
                schema: "public",
                table: "TranslatableFields",
                column: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "pk_translations",
                schema: "public",
                table: "Translations");

            migrationBuilder.DropPrimaryKey(
                name: "pk_translatable_fields",
                schema: "public",
                table: "TranslatableFields");

            migrationBuilder.RenameColumn(
                name: "lang",
                schema: "public",
                table: "Translations",
                newName: "Lang");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "public",
                table: "Translations",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "translated_value",
                schema: "public",
                table: "Translations",
                newName: "TranslatedValue");

            migrationBuilder.RenameColumn(
                name: "text_key",
                schema: "public",
                table: "Translations",
                newName: "TextKey");

            migrationBuilder.RenameColumn(
                name: "last_modified",
                schema: "public",
                table: "Translations",
                newName: "LastModified");

            migrationBuilder.RenameColumn(
                name: "is_stable",
                schema: "public",
                table: "Translations",
                newName: "IsStable");

            migrationBuilder.RenameColumn(
                name: "created_at",
                schema: "public",
                table: "Translations",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "public",
                table: "TranslatableFields",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "table_name",
                schema: "public",
                table: "TranslatableFields",
                newName: "TableName");

            migrationBuilder.RenameColumn(
                name: "is_active",
                schema: "public",
                table: "TranslatableFields",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "field_name",
                schema: "public",
                table: "TranslatableFields",
                newName: "FieldName");

            migrationBuilder.RenameColumn(
                name: "created_at",
                schema: "public",
                table: "TranslatableFields",
                newName: "CreatedAt");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Translations",
                schema: "public",
                table: "Translations",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TranslatableFields",
                schema: "public",
                table: "TranslatableFields",
                column: "Id");
        }
    }
}
