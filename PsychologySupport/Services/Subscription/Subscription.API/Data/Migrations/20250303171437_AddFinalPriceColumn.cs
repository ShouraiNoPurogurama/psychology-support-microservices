using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Subscription.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFinalPriceColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "FinalPrice",
                schema: "public",
                table: "UserSubscriptions",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FinalPrice",
                schema: "public",
                table: "UserSubscriptions");
        }
    }
}
