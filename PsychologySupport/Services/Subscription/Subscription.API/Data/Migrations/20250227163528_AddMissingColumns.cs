using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Subscription.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "GiftId",
                schema: "public",
                table: "UserSubscriptions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PromoCodeId",
                schema: "public",
                table: "UserSubscriptions",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserSubscriptions_ServicePackageId",
                schema: "public",
                table: "UserSubscriptions",
                column: "ServicePackageId");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserSubscriptions_ServicePackages_ServicePackageId",
                schema: "public",
                table: "UserSubscriptions");

            migrationBuilder.DropIndex(
                name: "IX_UserSubscriptions_ServicePackageId",
                schema: "public",
                table: "UserSubscriptions");

            migrationBuilder.DropColumn(
                name: "GiftId",
                schema: "public",
                table: "UserSubscriptions");

            migrationBuilder.DropColumn(
                name: "PromoCodeId",
                schema: "public",
                table: "UserSubscriptions");
        }
    }
}
