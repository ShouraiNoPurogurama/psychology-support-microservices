using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitalGoods.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEmotionTagScope : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "scope",
                table: "emotion_tags",
                type: "VARCHAR(25)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "scope",
                table: "emotion_tags");
        }
    }
}
