using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Profile.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveFixedColumnNameDefinitions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Gender",
                schema: "public",
                table: "patient_profiles",
                newName: "gender");

            migrationBuilder.RenameColumn(
                name: "PhoneNumber",
                schema: "public",
                table: "patient_profiles",
                newName: "contact_info_phone_number");

            migrationBuilder.RenameColumn(
                name: "PersonalityTraits",
                schema: "public",
                table: "patient_profiles",
                newName: "personality_traits");

            migrationBuilder.RenameColumn(
                name: "Email",
                schema: "public",
                table: "patient_profiles",
                newName: "contact_info_email");

            migrationBuilder.RenameColumn(
                name: "Address",
                schema: "public",
                table: "patient_profiles",
                newName: "contact_info_address");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "gender",
                schema: "public",
                table: "patient_profiles",
                newName: "Gender");

            migrationBuilder.RenameColumn(
                name: "personality_traits",
                schema: "public",
                table: "patient_profiles",
                newName: "PersonalityTraits");

            migrationBuilder.RenameColumn(
                name: "contact_info_phone_number",
                schema: "public",
                table: "patient_profiles",
                newName: "PhoneNumber");

            migrationBuilder.RenameColumn(
                name: "contact_info_email",
                schema: "public",
                table: "patient_profiles",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "contact_info_address",
                schema: "public",
                table: "patient_profiles",
                newName: "Address");
        }
    }
}
