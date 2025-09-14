using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Profile.API.Data.Pii.Migrations
{
    /// <inheritdoc />
    public partial class MappingIdToSubjectRef : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "address",
                schema: "pii",
                table: "person_profiles",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<int>(
                name: "status",
                schema: "pii",
                table: "person_profiles",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "status",
                schema: "pii",
                table: "person_profiles");

            migrationBuilder.AlterColumn<string>(
                name: "address",
                schema: "pii",
                table: "person_profiles",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
