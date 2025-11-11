using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auth.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFreeTrialTrackingToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_free_trial_used",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "valid_from",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "valid_to",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_free_trial_used",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "valid_from",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "valid_to",
                table: "AspNetUsers");
        }
    }
}
