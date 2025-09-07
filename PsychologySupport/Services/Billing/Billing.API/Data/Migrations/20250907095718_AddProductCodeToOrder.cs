using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Billing.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProductCodeToOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "product_code",
                table: "orders",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "product_code",
                table: "orders");
        }
    }
}
