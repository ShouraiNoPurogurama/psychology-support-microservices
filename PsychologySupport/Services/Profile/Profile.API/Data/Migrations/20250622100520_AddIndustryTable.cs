using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Profile.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIndustryTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Job_Industry_IndustryId",
                schema: "public",
                table: "Job");

            migrationBuilder.DropForeignKey(
                name: "FK_PatientProfiles_Job_JobId",
                schema: "public",
                table: "PatientProfiles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Job",
                schema: "public",
                table: "Job");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Industry",
                schema: "public",
                table: "Industry");

            migrationBuilder.RenameTable(
                name: "Job",
                schema: "public",
                newName: "Jobs",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "Industry",
                schema: "public",
                newName: "Industries",
                newSchema: "public");

            migrationBuilder.RenameIndex(
                name: "IX_Job_IndustryId",
                schema: "public",
                table: "Jobs",
                newName: "IX_Jobs_IndustryId");

            migrationBuilder.AlterColumn<string>(
                name: "EducationLevel",
                schema: "public",
                table: "Jobs",
                type: "VARCHAR(30)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Jobs",
                schema: "public",
                table: "Jobs",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Industries",
                schema: "public",
                table: "Industries",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Jobs_Industries_IndustryId",
                schema: "public",
                table: "Jobs",
                column: "IndustryId",
                principalSchema: "public",
                principalTable: "Industries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PatientProfiles_Jobs_JobId",
                schema: "public",
                table: "PatientProfiles",
                column: "JobId",
                principalSchema: "public",
                principalTable: "Jobs",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Jobs_Industries_IndustryId",
                schema: "public",
                table: "Jobs");

            migrationBuilder.DropForeignKey(
                name: "FK_PatientProfiles_Jobs_JobId",
                schema: "public",
                table: "PatientProfiles");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Jobs",
                schema: "public",
                table: "Jobs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Industries",
                schema: "public",
                table: "Industries");

            migrationBuilder.RenameTable(
                name: "Jobs",
                schema: "public",
                newName: "Job",
                newSchema: "public");

            migrationBuilder.RenameTable(
                name: "Industries",
                schema: "public",
                newName: "Industry",
                newSchema: "public");

            migrationBuilder.RenameIndex(
                name: "IX_Jobs_IndustryId",
                schema: "public",
                table: "Job",
                newName: "IX_Job_IndustryId");

            migrationBuilder.AlterColumn<int>(
                name: "EducationLevel",
                schema: "public",
                table: "Job",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "VARCHAR(30)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Job",
                schema: "public",
                table: "Job",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Industry",
                schema: "public",
                table: "Industry",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Job_Industry_IndustryId",
                schema: "public",
                table: "Job",
                column: "IndustryId",
                principalSchema: "public",
                principalTable: "Industry",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PatientProfiles_Job_JobId",
                schema: "public",
                table: "PatientProfiles",
                column: "JobId",
                principalSchema: "public",
                principalTable: "Job",
                principalColumn: "Id");
        }
    }
}
