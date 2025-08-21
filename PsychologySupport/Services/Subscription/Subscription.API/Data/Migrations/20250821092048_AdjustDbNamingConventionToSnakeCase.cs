using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Subscription.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AdjustDbNamingConventionToSnakeCase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserSubscriptions_ServicePackages_ServicePackageId",
                schema: "public",
                table: "UserSubscriptions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserSubscriptions",
                schema: "public",
                table: "UserSubscriptions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ServicePackages",
                schema: "public",
                table: "ServicePackages");

            migrationBuilder.RenameTable(
                name: "UserSubscriptions",
                schema: "public",
                newName: "user_subscriptions",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "ServicePackages",
                schema: "public",
                newName: "service_packages",
                newSchema: "public");

            migrationBuilder.RenameColumn(
                name: "Status",
                schema: "public",
                table: "user_subscriptions",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "public",
                table: "user_subscriptions",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "StartDate",
                schema: "public",
                table: "user_subscriptions",
                newName: "start_date");

            migrationBuilder.RenameColumn(
                name: "ServicePackageId",
                schema: "public",
                table: "user_subscriptions",
                newName: "service_package_id");

            migrationBuilder.RenameColumn(
                name: "PromoCodeId",
                schema: "public",
                table: "user_subscriptions",
                newName: "promo_code_id");

            migrationBuilder.RenameColumn(
                name: "PatientId",
                schema: "public",
                table: "user_subscriptions",
                newName: "patient_id");

            migrationBuilder.RenameColumn(
                name: "LastModifiedBy",
                schema: "public",
                table: "user_subscriptions",
                newName: "last_modified_by");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                schema: "public",
                table: "user_subscriptions",
                newName: "last_modified");

            migrationBuilder.RenameColumn(
                name: "GiftId",
                schema: "public",
                table: "user_subscriptions",
                newName: "gift_id");

            migrationBuilder.RenameColumn(
                name: "FinalPrice",
                schema: "public",
                table: "user_subscriptions",
                newName: "final_price");

            migrationBuilder.RenameColumn(
                name: "EndDate",
                schema: "public",
                table: "user_subscriptions",
                newName: "end_date");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                schema: "public",
                table: "user_subscriptions",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                schema: "public",
                table: "user_subscriptions",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "IX_UserSubscriptions_ServicePackageId",
                schema: "public",
                table: "user_subscriptions",
                newName: "ix_user_subscriptions_service_package_id");

            migrationBuilder.RenameColumn(
                name: "Price",
                schema: "public",
                table: "service_packages",
                newName: "price");

            migrationBuilder.RenameColumn(
                name: "Name",
                schema: "public",
                table: "service_packages",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Description",
                schema: "public",
                table: "service_packages",
                newName: "description");

            migrationBuilder.RenameColumn(
                name: "Id",
                schema: "public",
                table: "service_packages",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "LastModifiedBy",
                schema: "public",
                table: "service_packages",
                newName: "last_modified_by");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                schema: "public",
                table: "service_packages",
                newName: "last_modified");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                schema: "public",
                table: "service_packages",
                newName: "is_active");

            migrationBuilder.RenameColumn(
                name: "ImageId",
                schema: "public",
                table: "service_packages",
                newName: "image_id");

            migrationBuilder.RenameColumn(
                name: "DurationDays",
                schema: "public",
                table: "service_packages",
                newName: "duration_days");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                schema: "public",
                table: "service_packages",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                schema: "public",
                table: "service_packages",
                newName: "created_at");

            migrationBuilder.AddPrimaryKey(
                name: "pk_user_subscriptions",
                schema: "public",
                table: "user_subscriptions",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_service_packages",
                schema: "public",
                table: "service_packages",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_user_subscriptions_service_packages_service_package_id",
                schema: "public",
                table: "user_subscriptions",
                column: "service_package_id",
                principalSchema: "public",
                principalTable: "service_packages",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_user_subscriptions_service_packages_service_package_id",
                schema: "public",
                table: "user_subscriptions");

            migrationBuilder.DropPrimaryKey(
                name: "pk_user_subscriptions",
                schema: "public",
                table: "user_subscriptions");

            migrationBuilder.DropPrimaryKey(
                name: "pk_service_packages",
                schema: "public",
                table: "service_packages");

            migrationBuilder.RenameTable(
                name: "user_subscriptions",
                schema: "public",
                newName: "UserSubscriptions",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "service_packages",
                schema: "public",
                newName: "ServicePackages",
                newSchema: "public");

            migrationBuilder.RenameColumn(
                name: "status",
                schema: "public",
                table: "UserSubscriptions",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "public",
                table: "UserSubscriptions",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "start_date",
                schema: "public",
                table: "UserSubscriptions",
                newName: "StartDate");

            migrationBuilder.RenameColumn(
                name: "service_package_id",
                schema: "public",
                table: "UserSubscriptions",
                newName: "ServicePackageId");

            migrationBuilder.RenameColumn(
                name: "promo_code_id",
                schema: "public",
                table: "UserSubscriptions",
                newName: "PromoCodeId");

            migrationBuilder.RenameColumn(
                name: "patient_id",
                schema: "public",
                table: "UserSubscriptions",
                newName: "PatientId");

            migrationBuilder.RenameColumn(
                name: "last_modified_by",
                schema: "public",
                table: "UserSubscriptions",
                newName: "LastModifiedBy");

            migrationBuilder.RenameColumn(
                name: "last_modified",
                schema: "public",
                table: "UserSubscriptions",
                newName: "LastModified");

            migrationBuilder.RenameColumn(
                name: "gift_id",
                schema: "public",
                table: "UserSubscriptions",
                newName: "GiftId");

            migrationBuilder.RenameColumn(
                name: "final_price",
                schema: "public",
                table: "UserSubscriptions",
                newName: "FinalPrice");

            migrationBuilder.RenameColumn(
                name: "end_date",
                schema: "public",
                table: "UserSubscriptions",
                newName: "EndDate");

            migrationBuilder.RenameColumn(
                name: "created_by",
                schema: "public",
                table: "UserSubscriptions",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "created_at",
                schema: "public",
                table: "UserSubscriptions",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "ix_user_subscriptions_service_package_id",
                schema: "public",
                table: "UserSubscriptions",
                newName: "IX_UserSubscriptions_ServicePackageId");

            migrationBuilder.RenameColumn(
                name: "price",
                schema: "public",
                table: "ServicePackages",
                newName: "Price");

            migrationBuilder.RenameColumn(
                name: "name",
                schema: "public",
                table: "ServicePackages",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "description",
                schema: "public",
                table: "ServicePackages",
                newName: "Description");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "public",
                table: "ServicePackages",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "last_modified_by",
                schema: "public",
                table: "ServicePackages",
                newName: "LastModifiedBy");

            migrationBuilder.RenameColumn(
                name: "last_modified",
                schema: "public",
                table: "ServicePackages",
                newName: "LastModified");

            migrationBuilder.RenameColumn(
                name: "is_active",
                schema: "public",
                table: "ServicePackages",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "image_id",
                schema: "public",
                table: "ServicePackages",
                newName: "ImageId");

            migrationBuilder.RenameColumn(
                name: "duration_days",
                schema: "public",
                table: "ServicePackages",
                newName: "DurationDays");

            migrationBuilder.RenameColumn(
                name: "created_by",
                schema: "public",
                table: "ServicePackages",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "created_at",
                schema: "public",
                table: "ServicePackages",
                newName: "CreatedAt");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserSubscriptions",
                schema: "public",
                table: "UserSubscriptions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ServicePackages",
                schema: "public",
                table: "ServicePackages",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserSubscriptions_ServicePackages_ServicePackageId",
                schema: "public",
                table: "UserSubscriptions",
                column: "ServicePackageId",
                principalSchema: "public",
                principalTable: "ServicePackages",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
