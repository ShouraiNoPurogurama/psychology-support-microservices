using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Promotion.Grpc.Migrations
{
    /// <inheritdoc />
    public partial class AdjustDbNamingConventionToSnakeCase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GiftCodes_Promotions_PromotionId",
                table: "GiftCodes");

            migrationBuilder.DropForeignKey(
                name: "FK_PromoCodes_Promotions_PromotionId",
                table: "PromoCodes");

            migrationBuilder.DropForeignKey(
                name: "FK_Promotions_PromotionTypes_PromotionTypeId",
                table: "Promotions");

            migrationBuilder.DropForeignKey(
                name: "FK_PromotionServicePackages_Promotions_PromotionId",
                table: "PromotionServicePackages");

            migrationBuilder.DropForeignKey(
                name: "FK_PromotionTypeServicePackages_PromotionTypes_PromotionTypeId",
                table: "PromotionTypeServicePackages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Promotions",
                table: "Promotions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PromotionTypeServicePackages",
                table: "PromotionTypeServicePackages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PromotionTypes",
                table: "PromotionTypes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PromotionServicePackages",
                table: "PromotionServicePackages");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PromoCodes",
                table: "PromoCodes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_GiftCodes",
                table: "GiftCodes");

            migrationBuilder.RenameTable(
                name: "Promotions",
                newName: "promotions");

            migrationBuilder.RenameTable(
                name: "PromotionTypeServicePackages",
                newName: "promotion_type_service_packages");

            migrationBuilder.RenameTable(
                name: "PromotionTypes",
                newName: "promotion_types");

            migrationBuilder.RenameTable(
                name: "PromotionServicePackages",
                newName: "promotion_service_packages");

            migrationBuilder.RenameTable(
                name: "PromoCodes",
                newName: "promo_codes");

            migrationBuilder.RenameTable(
                name: "GiftCodes",
                newName: "gift_codes");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "promotions",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "promotions",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "PromotionTypeId",
                table: "promotions",
                newName: "promotion_type_id");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "promotions",
                newName: "is_active");

            migrationBuilder.RenameColumn(
                name: "ImageId",
                table: "promotions",
                newName: "image_id");

            migrationBuilder.RenameColumn(
                name: "EndDate",
                table: "promotions",
                newName: "end_date");

            migrationBuilder.RenameColumn(
                name: "EffectiveDate",
                table: "promotions",
                newName: "effective_date");

            migrationBuilder.RenameIndex(
                name: "IX_Promotions_PromotionTypeId",
                table: "promotions",
                newName: "ix_promotions_promotion_type_id");

            migrationBuilder.RenameColumn(
                name: "ServicePackageId",
                table: "promotion_type_service_packages",
                newName: "service_package_id");

            migrationBuilder.RenameColumn(
                name: "PromotionTypeId",
                table: "promotion_type_service_packages",
                newName: "promotion_type_id");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "promotion_types",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "promotion_types",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "promotion_types",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "ServicePackageId",
                table: "promotion_service_packages",
                newName: "service_package_id");

            migrationBuilder.RenameColumn(
                name: "PromotionId",
                table: "promotion_service_packages",
                newName: "promotion_id");

            migrationBuilder.RenameColumn(
                name: "Value",
                table: "promo_codes",
                newName: "value");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "promo_codes",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "promo_codes",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "Code",
                table: "promo_codes",
                newName: "code");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "promo_codes",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "PromotionId",
                table: "promo_codes",
                newName: "promotion_id");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "promo_codes",
                newName: "is_active");

            migrationBuilder.RenameIndex(
                name: "IX_PromoCodes_PromotionId",
                table: "promo_codes",
                newName: "ix_promo_codes_promotion_id");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "gift_codes",
                newName: "title");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "gift_codes",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "Code",
                table: "gift_codes",
                newName: "code");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "gift_codes",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "PromotionId",
                table: "gift_codes",
                newName: "promotion_id");

            migrationBuilder.RenameColumn(
                name: "PatientId",
                table: "gift_codes",
                newName: "patient_id");

            migrationBuilder.RenameColumn(
                name: "MoneyValue",
                table: "gift_codes",
                newName: "money_value");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "gift_codes",
                newName: "is_active");

            migrationBuilder.RenameIndex(
                name: "IX_GiftCodes_PromotionId",
                table: "gift_codes",
                newName: "ix_gift_codes_promotion_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_promotions",
                table: "promotions",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_promotion_type_service_packages",
                table: "promotion_type_service_packages",
                columns: new[] { "promotion_type_id", "service_package_id" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_promotion_types",
                table: "promotion_types",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_promotion_service_packages",
                table: "promotion_service_packages",
                columns: new[] { "promotion_id", "service_package_id" });

            migrationBuilder.AddPrimaryKey(
                name: "pk_promo_codes",
                table: "promo_codes",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_gift_codes",
                table: "gift_codes",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_gift_codes_promotions_promotion_id",
                table: "gift_codes",
                column: "promotion_id",
                principalTable: "promotions",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_promo_codes_promotions_promotion_id",
                table: "promo_codes",
                column: "promotion_id",
                principalTable: "promotions",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_promotion_service_packages_promotions_promotion_id",
                table: "promotion_service_packages",
                column: "promotion_id",
                principalTable: "promotions",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_promotion_type_service_packages_promotion_types_promotion_t",
                table: "promotion_type_service_packages",
                column: "promotion_type_id",
                principalTable: "promotion_types",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_promotions_promotion_types_promotion_type_id",
                table: "promotions",
                column: "promotion_type_id",
                principalTable: "promotion_types",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_gift_codes_promotions_promotion_id",
                table: "gift_codes");

            migrationBuilder.DropForeignKey(
                name: "fk_promo_codes_promotions_promotion_id",
                table: "promo_codes");

            migrationBuilder.DropForeignKey(
                name: "fk_promotion_service_packages_promotions_promotion_id",
                table: "promotion_service_packages");

            migrationBuilder.DropForeignKey(
                name: "fk_promotion_type_service_packages_promotion_types_promotion_t",
                table: "promotion_type_service_packages");

            migrationBuilder.DropForeignKey(
                name: "fk_promotions_promotion_types_promotion_type_id",
                table: "promotions");

            migrationBuilder.DropPrimaryKey(
                name: "pk_promotions",
                table: "promotions");

            migrationBuilder.DropPrimaryKey(
                name: "pk_promotion_types",
                table: "promotion_types");

            migrationBuilder.DropPrimaryKey(
                name: "pk_promotion_type_service_packages",
                table: "promotion_type_service_packages");

            migrationBuilder.DropPrimaryKey(
                name: "pk_promotion_service_packages",
                table: "promotion_service_packages");

            migrationBuilder.DropPrimaryKey(
                name: "pk_promo_codes",
                table: "promo_codes");

            migrationBuilder.DropPrimaryKey(
                name: "pk_gift_codes",
                table: "gift_codes");

            migrationBuilder.RenameTable(
                name: "promotions",
                newName: "Promotions");

            migrationBuilder.RenameTable(
                name: "promotion_types",
                newName: "PromotionTypes");

            migrationBuilder.RenameTable(
                name: "promotion_type_service_packages",
                newName: "PromotionTypeServicePackages");

            migrationBuilder.RenameTable(
                name: "promotion_service_packages",
                newName: "PromotionServicePackages");

            migrationBuilder.RenameTable(
                name: "promo_codes",
                newName: "PromoCodes");

            migrationBuilder.RenameTable(
                name: "gift_codes",
                newName: "GiftCodes");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "Promotions",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Promotions",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "promotion_type_id",
                table: "Promotions",
                newName: "PromotionTypeId");

            migrationBuilder.RenameColumn(
                name: "is_active",
                table: "Promotions",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "image_id",
                table: "Promotions",
                newName: "ImageId");

            migrationBuilder.RenameColumn(
                name: "end_date",
                table: "Promotions",
                newName: "EndDate");

            migrationBuilder.RenameColumn(
                name: "effective_date",
                table: "Promotions",
                newName: "EffectiveDate");

            migrationBuilder.RenameIndex(
                name: "ix_promotions_promotion_type_id",
                table: "Promotions",
                newName: "IX_Promotions_PromotionTypeId");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "PromotionTypes",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "PromotionTypes",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "PromotionTypes",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "service_package_id",
                table: "PromotionTypeServicePackages",
                newName: "ServicePackageId");

            migrationBuilder.RenameColumn(
                name: "promotion_type_id",
                table: "PromotionTypeServicePackages",
                newName: "PromotionTypeId");

            migrationBuilder.RenameColumn(
                name: "service_package_id",
                table: "PromotionServicePackages",
                newName: "ServicePackageId");

            migrationBuilder.RenameColumn(
                name: "promotion_id",
                table: "PromotionServicePackages",
                newName: "PromotionId");

            migrationBuilder.RenameColumn(
                name: "value",
                table: "PromoCodes",
                newName: "Value");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "PromoCodes",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "PromoCodes",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "code",
                table: "PromoCodes",
                newName: "Code");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "PromoCodes",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "promotion_id",
                table: "PromoCodes",
                newName: "PromotionId");

            migrationBuilder.RenameColumn(
                name: "is_active",
                table: "PromoCodes",
                newName: "IsActive");

            migrationBuilder.RenameIndex(
                name: "ix_promo_codes_promotion_id",
                table: "PromoCodes",
                newName: "IX_PromoCodes_PromotionId");

            migrationBuilder.RenameColumn(
                name: "title",
                table: "GiftCodes",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "description",
                table: "GiftCodes",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "code",
                table: "GiftCodes",
                newName: "Code");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "GiftCodes",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "promotion_id",
                table: "GiftCodes",
                newName: "PromotionId");

            migrationBuilder.RenameColumn(
                name: "patient_id",
                table: "GiftCodes",
                newName: "PatientId");

            migrationBuilder.RenameColumn(
                name: "money_value",
                table: "GiftCodes",
                newName: "MoneyValue");

            migrationBuilder.RenameColumn(
                name: "is_active",
                table: "GiftCodes",
                newName: "IsActive");

            migrationBuilder.RenameIndex(
                name: "ix_gift_codes_promotion_id",
                table: "GiftCodes",
                newName: "IX_GiftCodes_PromotionId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Promotions",
                table: "Promotions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PromotionTypes",
                table: "PromotionTypes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PromotionTypeServicePackages",
                table: "PromotionTypeServicePackages",
                columns: new[] { "PromotionTypeId", "ServicePackageId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_PromotionServicePackages",
                table: "PromotionServicePackages",
                columns: new[] { "PromotionId", "ServicePackageId" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_PromoCodes",
                table: "PromoCodes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_GiftCodes",
                table: "GiftCodes",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GiftCodes_Promotions_PromotionId",
                table: "GiftCodes",
                column: "PromotionId",
                principalTable: "Promotions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PromoCodes_Promotions_PromotionId",
                table: "PromoCodes",
                column: "PromotionId",
                principalTable: "Promotions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Promotions_PromotionTypes_PromotionTypeId",
                table: "Promotions",
                column: "PromotionTypeId",
                principalTable: "PromotionTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PromotionServicePackages_Promotions_PromotionId",
                table: "PromotionServicePackages",
                column: "PromotionId",
                principalTable: "Promotions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PromotionTypeServicePackages_PromotionTypes_PromotionTypeId",
                table: "PromotionTypeServicePackages",
                column: "PromotionTypeId",
                principalTable: "PromotionTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
