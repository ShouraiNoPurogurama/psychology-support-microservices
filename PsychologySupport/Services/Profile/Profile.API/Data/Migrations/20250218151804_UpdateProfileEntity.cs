using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Profile.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProfileEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FullName",
                schema: "public",
                table: "PatientProfiles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                schema: "public",
                table: "DoctorProfiles",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FullName",
                schema: "public",
                table: "PatientProfiles");

            migrationBuilder.DropColumn(
                name: "FullName",
                schema: "public",
                table: "DoctorProfiles");
        }
    }
}
