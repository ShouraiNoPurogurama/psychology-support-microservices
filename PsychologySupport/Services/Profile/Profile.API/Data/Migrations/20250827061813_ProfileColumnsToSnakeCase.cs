using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Profile.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class ProfileColumnsToSnakeCase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "contact_info_phone_number",
                schema: "public",
                table: "patient_profiles",
                newName: "phone_number");

            migrationBuilder.RenameColumn(
                name: "contact_info_email",
                schema: "public",
                table: "patient_profiles",
                newName: "email");

            migrationBuilder.RenameColumn(
                name: "contact_info_address",
                schema: "public",
                table: "patient_profiles",
                newName: "address");

            migrationBuilder.RenameColumn(
                name: "Gender",
                schema: "public",
                table: "doctor_profiles",
                newName: "gender");

            migrationBuilder.RenameColumn(
                name: "Email",
                schema: "public",
                table: "doctor_profiles",
                newName: "email");

            migrationBuilder.RenameColumn(
                name: "Address",
                schema: "public",
                table: "doctor_profiles",
                newName: "address");

            migrationBuilder.RenameColumn(
                name: "PhoneNumber",
                schema: "public",
                table: "doctor_profiles",
                newName: "phone_number");

            migrationBuilder.AlterColumn<string>(
                name: "gender",
                schema: "public",
                table: "doctor_profiles",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "phone_number",
                schema: "public",
                table: "patient_profiles",
                newName: "contact_info_phone_number");

            migrationBuilder.RenameColumn(
                name: "email",
                schema: "public",
                table: "patient_profiles",
                newName: "contact_info_email");

            migrationBuilder.RenameColumn(
                name: "address",
                schema: "public",
                table: "patient_profiles",
                newName: "contact_info_address");

            migrationBuilder.RenameColumn(
                name: "gender",
                schema: "public",
                table: "doctor_profiles",
                newName: "Gender");

            migrationBuilder.RenameColumn(
                name: "email",
                schema: "public",
                table: "doctor_profiles",
                newName: "Email");

            migrationBuilder.RenameColumn(
                name: "address",
                schema: "public",
                table: "doctor_profiles",
                newName: "Address");

            migrationBuilder.RenameColumn(
                name: "phone_number",
                schema: "public",
                table: "doctor_profiles",
                newName: "PhoneNumber");

            migrationBuilder.AlterColumn<string>(
                name: "Gender",
                schema: "public",
                table: "doctor_profiles",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
