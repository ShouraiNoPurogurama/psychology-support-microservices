using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Profile.API.Data.Public.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSubjectRef : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "subject_ref",
                schema: "public",
                table: "patient_profiles");

            // migrationBuilder.AlterColumn<string>(
            //     name: "address",
            //     schema: "public",
            //     table: "doctor_profiles",
            //     type: "text",
            //     nullable: true,
            //     oldClrType: typeof(string),
            //     oldType: "text");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "subject_ref",
                schema: "public",
                table: "patient_profiles",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            // migrationBuilder.AlterColumn<string>(
            //     name: "address",
            //     schema: "public",
            //     table: "doctor_profiles",
            //     type: "text",
            //     nullable: false,
            //     defaultValue: "",
            //     oldClrType: typeof(string),
            //     oldType: "text",
            //     oldNullable: true);
        }
    }
}
