using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LifeStyles.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class SplitEmotionFeaturesIntoManyTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CurrentEmotions",
                schema: "public");

            migrationBuilder.CreateTable(
                name: "Emotions",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IconUrl = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Emotions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PatientEmotionCheckpoints",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    LogDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientEmotionCheckpoints", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EmotionSelections",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmotionCheckpointId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmotionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Intensity = table.Column<int>(type: "integer", nullable: true),
                    Rank = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmotionSelections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmotionSelections_Emotions_EmotionId",
                        column: x => x.EmotionId,
                        principalSchema: "public",
                        principalTable: "Emotions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmotionSelections_PatientEmotionCheckpoints_EmotionCheckpoi~",
                        column: x => x.EmotionCheckpointId,
                        principalSchema: "public",
                        principalTable: "PatientEmotionCheckpoints",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmotionSelections_EmotionCheckpointId",
                schema: "public",
                table: "EmotionSelections",
                column: "EmotionCheckpointId");

            migrationBuilder.CreateIndex(
                name: "IX_EmotionSelections_EmotionId",
                schema: "public",
                table: "EmotionSelections",
                column: "EmotionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmotionSelections",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Emotions",
                schema: "public");

            migrationBuilder.DropTable(
                name: "PatientEmotionCheckpoints",
                schema: "public");

            migrationBuilder.CreateTable(
                name: "CurrentEmotions",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Emotion1 = table.Column<string>(type: "VARCHAR(20)", nullable: true),
                    Emotion2 = table.Column<string>(type: "VARCHAR(20)", nullable: true),
                    LogDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PatientProfileId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurrentEmotions", x => x.Id);
                });
        }
    }
}
