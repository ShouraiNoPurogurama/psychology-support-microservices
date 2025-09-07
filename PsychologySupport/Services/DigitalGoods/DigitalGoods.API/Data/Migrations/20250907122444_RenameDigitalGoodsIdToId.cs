using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitalGoods.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameDigitalGoodsIdToId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "inventory_id",
                table: "inventories",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "digital_good_id",
                table: "digital_goods",
                newName: "id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "id",
                table: "inventories",
                newName: "inventory_id");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "digital_goods",
                newName: "digital_good_id");
        }
    }
}
