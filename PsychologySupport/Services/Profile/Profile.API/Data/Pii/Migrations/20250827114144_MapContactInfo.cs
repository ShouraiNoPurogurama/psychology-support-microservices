using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Profile.API.Data.Pii.Migrations
{
    /// <inheritdoc />
    public partial class MapContactInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.DropColumn(
            //     name: "current_address",
            //     schema: "pii",
            //     table: "person_profiles");
            //
            // migrationBuilder.RenameColumn(
            //     name: "primary_phone",
            //     schema: "pii",
            //     table: "person_profiles",
            //     newName: "phone_number");
            //
            // migrationBuilder.AddColumn<string>(
            //     name: "address",
            //     schema: "pii",
            //     table: "person_profiles",
            //     type: "text",
            //     nullable: false,
            //     defaultValue: "");
            //
            // migrationBuilder.AddColumn<string>(
            //     name: "email",
            //     schema: "pii",
            //     table: "person_profiles",
            //     type: "text",
            //     nullable: false,
            //     defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.DropColumn(
            //     name: "address",
            //     schema: "pii",
            //     table: "person_profiles");
            //
            // migrationBuilder.DropColumn(
            //     name: "email",
            //     schema: "pii",
            //     table: "person_profiles");

            migrationBuilder.RenameColumn(
                name: "phone_number",
                schema: "pii",
                table: "person_profiles",
                newName: "primary_phone");

            migrationBuilder.AddColumn<string>(
                name: "current_address",
                schema: "pii",
                table: "person_profiles",
                type: "text",
                nullable: true);
        }
    }
}
