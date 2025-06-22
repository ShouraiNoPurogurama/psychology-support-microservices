using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Profile.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProfileModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PersonalityTraits",
                schema: "public",
                table: "PatientProfiles",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "Gender",
                schema: "public",
                table: "PatientProfiles",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10);

            migrationBuilder.AddColumn<DateOnly>(
                name: "BirthDate",
                schema: "public",
                table: "PatientProfiles",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "JobId",
                schema: "public",
                table: "PatientProfiles",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Industry",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IndustryName = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Industry", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Job",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IndustryId = table.Column<Guid>(type: "uuid", nullable: false),
                    JobTitle = table.Column<string>(type: "text", nullable: false),
                    EducationLevel = table.Column<string>(type: "VARCHAR(30)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Job", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Job_Industry_IndustryId",
                        column: x => x.IndustryId,
                        principalSchema: "public",
                        principalTable: "Industry",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PatientProfiles_JobId",
                schema: "public",
                table: "PatientProfiles",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_Job_IndustryId",
                schema: "public",
                table: "Job",
                column: "IndustryId");

            migrationBuilder.AddForeignKey(
                name: "FK_PatientProfiles_Job_JobId",
                schema: "public",
                table: "PatientProfiles",
                column: "JobId",
                principalSchema: "public",
                principalTable: "Job",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PatientProfiles_Job_JobId",
                schema: "public",
                table: "PatientProfiles");

            migrationBuilder.DropTable(
                name: "Job",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Industry",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_PatientProfiles_JobId",
                schema: "public",
                table: "PatientProfiles");

            migrationBuilder.DropColumn(
                name: "BirthDate",
                schema: "public",
                table: "PatientProfiles");

            migrationBuilder.DropColumn(
                name: "JobId",
                schema: "public",
                table: "PatientProfiles");

            migrationBuilder.AlterColumn<string>(
                name: "PersonalityTraits",
                schema: "public",
                table: "PatientProfiles",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Gender",
                schema: "public",
                table: "PatientProfiles",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
