using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LifeStyles.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPatientFoodAndTheraActivities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PatientFoodActivities",
                schema: "public",
                columns: table => new
                {
                    PatientProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    FoodActivityId = table.Column<Guid>(type: "uuid", nullable: false),
                    PreferenceLevel = table.Column<string>(type: "VARCHAR(20)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientFoodActivities", x => new { x.PatientProfileId, x.FoodActivityId });
                });

            migrationBuilder.CreateTable(
                name: "PatientTherapeuticActivities",
                schema: "public",
                columns: table => new
                {
                    PatientProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    TherapeuticActivityId = table.Column<Guid>(type: "uuid", nullable: false),
                    PreferenceLevel = table.Column<string>(type: "VARCHAR(20)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientTherapeuticActivities", x => new { x.PatientProfileId, x.TherapeuticActivityId });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PatientFoodActivities",
                schema: "public");

            migrationBuilder.DropTable(
                name: "PatientTherapeuticActivities",
                schema: "public");
        }
    }
}
