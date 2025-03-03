using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Promotion.Grpc.Data.Migrations
{
    /// <inheritdoc />
    public partial class AdjustGiftCodeTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PatientId",
                table: "GiftCodes",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PatientId",
                table: "GiftCodes");
        }
    }
}
