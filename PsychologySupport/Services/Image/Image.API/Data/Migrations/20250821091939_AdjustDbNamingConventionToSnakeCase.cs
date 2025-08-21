using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Image.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AdjustDbNamingConventionToSnakeCase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Images",
                schema: "public",
                table: "Images");

            migrationBuilder.RenameTable(
                name: "Images",
                schema: "public",
                newName: "images",
                newSchema: "public");

            migrationBuilder.RenameColumn(
                name: "Url",
                schema: "public",
                table: "images",
                newName: "url");

            migrationBuilder.RenameColumn(
                name: "Name",
                schema: "public",
                table: "images",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Extension",
                schema: "public",
                table: "images",
                newName: "extension");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "public",
                table: "images",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "OwnerType",
                schema: "public",
                table: "images",
                newName: "owner_type");

            migrationBuilder.RenameColumn(
                name: "OwnerId",
                schema: "public",
                table: "images",
                newName: "owner_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_images",
                schema: "public",
                table: "images",
                column: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "pk_images",
                schema: "public",
                table: "images");

            migrationBuilder.RenameTable(
                name: "images",
                schema: "public",
                newName: "Images",
                newSchema: "public");

            migrationBuilder.RenameColumn(
                name: "url",
                schema: "public",
                table: "Images",
                newName: "Url");

            migrationBuilder.RenameColumn(
                name: "name",
                schema: "public",
                table: "Images",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "extension",
                schema: "public",
                table: "Images",
                newName: "Extension");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "public",
                table: "Images",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "owner_type",
                schema: "public",
                table: "Images",
                newName: "OwnerType");

            migrationBuilder.RenameColumn(
                name: "owner_id",
                schema: "public",
                table: "Images",
                newName: "OwnerId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Images",
                schema: "public",
                table: "Images",
                column: "Id");
        }
    }
}
