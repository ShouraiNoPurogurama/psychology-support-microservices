using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Profile.API.Data.Public.Migrations
{
    /// <inheritdoc />
    public partial class AddAudiableColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "created_at",
                schema: "public",
                table: "patient_profiles",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "created_by",
                schema: "public",
                table: "patient_profiles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "last_modified",
                schema: "public",
                table: "patient_profiles",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "last_modified_by",
                schema: "public",
                table: "patient_profiles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "created_at",
                schema: "public",
                table: "doctor_profiles",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "created_by",
                schema: "public",
                table: "doctor_profiles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "last_modified",
                schema: "public",
                table: "doctor_profiles",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "last_modified_by",
                schema: "public",
                table: "doctor_profiles",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "created_at",
                schema: "public",
                table: "patient_profiles");

            migrationBuilder.DropColumn(
                name: "created_by",
                schema: "public",
                table: "patient_profiles");

            migrationBuilder.DropColumn(
                name: "last_modified",
                schema: "public",
                table: "patient_profiles");

            migrationBuilder.DropColumn(
                name: "last_modified_by",
                schema: "public",
                table: "patient_profiles");

            migrationBuilder.DropColumn(
                name: "created_at",
                schema: "public",
                table: "doctor_profiles");

            migrationBuilder.DropColumn(
                name: "created_by",
                schema: "public",
                table: "doctor_profiles");

            migrationBuilder.DropColumn(
                name: "last_modified",
                schema: "public",
                table: "doctor_profiles");

            migrationBuilder.DropColumn(
                name: "last_modified_by",
                schema: "public",
                table: "doctor_profiles");
        }
    }
}
