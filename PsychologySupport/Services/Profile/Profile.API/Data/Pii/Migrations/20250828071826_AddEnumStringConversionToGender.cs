using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Profile.API.Data.Pii.Migrations
{
    /// <inheritdoc />
    public partial class AddEnumStringConversionToGender : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "gender",
                schema: "pii",
                table: "person_profiles",
                type: "text",
                nullable: false,
                defaultValue: "Else",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "fk_alias_owner_map_person_profiles_user_id",
                schema: "pii",
                table: "alias_owner_map",
                column: "user_id",
                principalSchema: "pii",
                principalTable: "person_profiles",
                principalColumn: "user_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_alias_owner_map_person_profiles_user_id",
                schema: "pii",
                table: "alias_owner_map");

            migrationBuilder.AlterColumn<string>(
                name: "gender",
                schema: "pii",
                table: "person_profiles",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "Else");
        }
    }
}
