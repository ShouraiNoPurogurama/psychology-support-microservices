using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LifeStyles.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCurrentEmotionLifestyleLogAndImprovementGoals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CurrentEmotions",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    LogDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Emotion1 = table.Column<string>(type: "VARCHAR(20)", nullable: true),
                    Emotion2 = table.Column<string>(type: "VARCHAR(20)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurrentEmotions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ImprovementGoals",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImprovementGoals", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LifestyleLogs",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    LogDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    SleepHours = table.Column<string>(type: "VARCHAR(20)", nullable: false),
                    ExerciseFrequency = table.Column<string>(type: "VARCHAR(20)", nullable: false),
                    AvailableTimePerDay = table.Column<string>(type: "VARCHAR(20)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LifestyleLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PatientImprovementGoals",
                schema: "public",
                columns: table => new
                {
                    PatientProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    GoalId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientImprovementGoals", x => new { x.PatientProfileId, x.GoalId });
                    table.ForeignKey(
                        name: "FK_PatientImprovementGoals_ImprovementGoals_GoalId",
                        column: x => x.GoalId,
                        principalSchema: "public",
                        principalTable: "ImprovementGoals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PatientImprovementGoals_GoalId",
                schema: "public",
                table: "PatientImprovementGoals",
                column: "GoalId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CurrentEmotions",
                schema: "public");

            migrationBuilder.DropTable(
                name: "LifestyleLogs",
                schema: "public");

            migrationBuilder.DropTable(
                name: "PatientImprovementGoals",
                schema: "public");

            migrationBuilder.DropTable(
                name: "ImprovementGoals",
                schema: "public");
        }
    }
}
