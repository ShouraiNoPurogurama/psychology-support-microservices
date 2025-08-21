#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace Promotion.Grpc.Migrations
{
    /// <inheritdoc />
    public partial class AddPromotionName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Promotions",
                type: "text",
                nullable: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "Promotions");
        }
    }
}