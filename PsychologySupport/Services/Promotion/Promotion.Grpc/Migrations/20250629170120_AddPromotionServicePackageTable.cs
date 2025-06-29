using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Promotion.Grpc.Migrations
{
    /// <inheritdoc />
    public partial class AddPromotionServicePackageTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PromotionServicePackages",
                columns: table => new
                {
                    PromotionId = table.Column<string>(type: "text", nullable: false),
                    ServicePackageId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromotionServicePackages", x => new { x.PromotionId, x.ServicePackageId });
                    table.ForeignKey(
                        name: "FK_PromotionServicePackages_Promotions_PromotionId",
                        column: x => x.PromotionId,
                        principalTable: "Promotions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PromotionServicePackages");
        }
    }
}
