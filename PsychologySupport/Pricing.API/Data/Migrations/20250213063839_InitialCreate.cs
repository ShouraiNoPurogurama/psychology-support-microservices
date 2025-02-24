using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pricing.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.CreateTable(
                name: "AcademicLevelSalaryRatios",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AcademicLevel = table.Column<int>(type: "integer", nullable: false),
                    FeeMultiplier = table.Column<decimal>(type: "numeric", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AcademicLevelSalaryRatios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExperiencePriceRanges",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MinYearsOfExperience = table.Column<int>(type: "integer", nullable: false),
                    MaxYearsOfExperience = table.Column<int>(type: "integer", nullable: false),
                    PricePerSession = table.Column<decimal>(type: "numeric", nullable: false),
                    PricePerMinute = table.Column<decimal>(type: "numeric", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExperiencePriceRanges", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AcademicLevelSalaryRatios",
                schema: "public");

            migrationBuilder.DropTable(
                name: "ExperiencePriceRanges",
                schema: "public");
        }
    }
}
