using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LifeStyles.API.Data.Migrations
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
                name: "EntertainmentActivities",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    IntensityLevel = table.Column<string>(type: "VARCHAR(20)", nullable: false),
                    ImpactLevel = table.Column<string>(type: "VARCHAR(20)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EntertainmentActivities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FoodActivities",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    MealTime = table.Column<int>(type: "VARCHAR(20)", nullable: false),
                    IntensityLevel = table.Column<string>(type: "VARCHAR(20)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FoodActivities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FoodCategories",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FoodCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FoodNutrients",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FoodNutrients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PatientEntertainmentActivities",
                schema: "public",
                columns: table => new
                {
                    PatientProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    EntertainmentActivityId = table.Column<Guid>(type: "uuid", nullable: false),
                    PreferenceLevel = table.Column<string>(type: "VARCHAR(20)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientEntertainmentActivities", x => new { x.PatientProfileId, x.EntertainmentActivityId });
                });

            migrationBuilder.CreateTable(
                name: "PatientPhysicalActivities",
                schema: "public",
                columns: table => new
                {
                    PatientProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    PhysicalActivityId = table.Column<Guid>(type: "uuid", nullable: false),
                    PreferenceLevel = table.Column<string>(type: "VARCHAR(20)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientPhysicalActivities", x => new { x.PatientProfileId, x.PhysicalActivityId });
                });

            migrationBuilder.CreateTable(
                name: "PhysicalActivities",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    IntensityLevel = table.Column<string>(type: "VARCHAR(20)", nullable: false),
                    ImpactLevel = table.Column<int>(type: "VARCHAR(20)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhysicalActivities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TherapeuticTypes",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TherapeuticTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FoodActivityFoodCategory",
                schema: "public",
                columns: table => new
                {
                    FoodActivitiesId = table.Column<Guid>(type: "uuid", nullable: false),
                    FoodCategoriesId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FoodActivityFoodCategory", x => new { x.FoodActivitiesId, x.FoodCategoriesId });
                    table.ForeignKey(
                        name: "FK_FoodActivityFoodCategory_FoodActivities_FoodActivitiesId",
                        column: x => x.FoodActivitiesId,
                        principalSchema: "public",
                        principalTable: "FoodActivities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FoodActivityFoodCategory_FoodCategories_FoodCategoriesId",
                        column: x => x.FoodCategoriesId,
                        principalSchema: "public",
                        principalTable: "FoodCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FoodActivityFoodNutrient",
                schema: "public",
                columns: table => new
                {
                    FoodActivitiesId = table.Column<Guid>(type: "uuid", nullable: false),
                    FoodNutrientsId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FoodActivityFoodNutrient", x => new { x.FoodActivitiesId, x.FoodNutrientsId });
                    table.ForeignKey(
                        name: "FK_FoodActivityFoodNutrient_FoodActivities_FoodActivitiesId",
                        column: x => x.FoodActivitiesId,
                        principalSchema: "public",
                        principalTable: "FoodActivities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FoodActivityFoodNutrient_FoodNutrients_FoodNutrientsId",
                        column: x => x.FoodNutrientsId,
                        principalSchema: "public",
                        principalTable: "FoodNutrients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TherapeuticActivities",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TherapeuticTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Instructions = table.Column<string>(type: "text", nullable: false),
                    IntensityLevel = table.Column<string>(type: "VARCHAR(20)", nullable: false),
                    ImpactLevel = table.Column<string>(type: "VARCHAR(20)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TherapeuticActivities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TherapeuticActivities_TherapeuticTypes_TherapeuticTypeId",
                        column: x => x.TherapeuticTypeId,
                        principalSchema: "public",
                        principalTable: "TherapeuticTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FoodActivityFoodCategory_FoodCategoriesId",
                schema: "public",
                table: "FoodActivityFoodCategory",
                column: "FoodCategoriesId");

            migrationBuilder.CreateIndex(
                name: "IX_FoodActivityFoodNutrient_FoodNutrientsId",
                schema: "public",
                table: "FoodActivityFoodNutrient",
                column: "FoodNutrientsId");

            migrationBuilder.CreateIndex(
                name: "IX_TherapeuticActivities_TherapeuticTypeId",
                schema: "public",
                table: "TherapeuticActivities",
                column: "TherapeuticTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EntertainmentActivities",
                schema: "public");

            migrationBuilder.DropTable(
                name: "FoodActivityFoodCategory",
                schema: "public");

            migrationBuilder.DropTable(
                name: "FoodActivityFoodNutrient",
                schema: "public");

            migrationBuilder.DropTable(
                name: "PatientEntertainmentActivities",
                schema: "public");

            migrationBuilder.DropTable(
                name: "PatientPhysicalActivities",
                schema: "public");

            migrationBuilder.DropTable(
                name: "PhysicalActivities",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TherapeuticActivities",
                schema: "public");

            migrationBuilder.DropTable(
                name: "FoodCategories",
                schema: "public");

            migrationBuilder.DropTable(
                name: "FoodActivities",
                schema: "public");

            migrationBuilder.DropTable(
                name: "FoodNutrients",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TherapeuticTypes",
                schema: "public");
        }
    }
}
