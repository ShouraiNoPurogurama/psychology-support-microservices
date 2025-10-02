using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitalGoods.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUnicodeCodepoint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "unicode_codepoint",
                table: "emotion_tags",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "unicode_codepoint",
                table: "emotion_tags");
        }
    }
}
