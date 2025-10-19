using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Subscription.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOriginalPriceAndDiscountLabel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.AddColumn<string>(
                name: "discount_label",
                schema: "public",
                table: "service_packages",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "original_price",
                schema: "public",
                table: "service_packages",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "discount_label",
                schema: "public",
                table: "service_packages");

            migrationBuilder.DropColumn(
                name: "original_price",
                schema: "public",
                table: "service_packages");
        }
    }
}
