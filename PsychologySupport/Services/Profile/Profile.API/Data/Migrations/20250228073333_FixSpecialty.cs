using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Profile.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixSpecialty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DoctorProfileSpecialty_Specialty_SpecialtiesId",
                schema: "public",
                table: "DoctorProfileSpecialty");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Specialty",
                schema: "public",
                table: "Specialty");

            migrationBuilder.RenameTable(
                name: "Specialty",
                schema: "public",
                newName: "Specialties",
                newSchema: "public");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Specialties",
                schema: "public",
                table: "Specialties",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DoctorProfileSpecialty_Specialties_SpecialtiesId",
                schema: "public",
                table: "DoctorProfileSpecialty",
                column: "SpecialtiesId",
                principalSchema: "public",
                principalTable: "Specialties",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DoctorProfileSpecialty_Specialties_SpecialtiesId",
                schema: "public",
                table: "DoctorProfileSpecialty");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Specialties",
                schema: "public",
                table: "Specialties");

            migrationBuilder.RenameTable(
                name: "Specialties",
                schema: "public",
                newName: "Specialty",
                newSchema: "public");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Specialty",
                schema: "public",
                table: "Specialty",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DoctorProfileSpecialty_Specialty_SpecialtiesId",
                schema: "public",
                table: "DoctorProfileSpecialty",
                column: "SpecialtiesId",
                principalSchema: "public",
                principalTable: "Specialty",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
