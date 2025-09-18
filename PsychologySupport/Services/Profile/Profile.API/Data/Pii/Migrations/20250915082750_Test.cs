using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Profile.API.Data.Pii.Migrations
{
    /// <inheritdoc />
    public partial class Test : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.DropForeignKey(
            //     name: "fk_alias_owner_map_person_profiles_subject_ref",
            //     schema: "pii",
            //     table: "alias_owner_map");

            migrationBuilder.DropForeignKey(
                name: "fk_patient_owner_map_person_profiles_subject_ref",
                schema: "pii",
                table: "patient_owner_map");

            migrationBuilder.DropPrimaryKey(
                name: "pk_person_profiles_subject_ref",
                schema: "pii",
                table: "person_profiles");

            migrationBuilder.AddPrimaryKey(
                name: "ak_person_profiles_subject_ref",
                schema: "pii",
                table: "person_profiles",
                column: "subject_ref");

            migrationBuilder.AddForeignKey(
                name: "fk_alias_owner_map_person_profile",
                schema: "pii",
                table: "alias_owner_map",
                column: "subject_ref",
                principalSchema: "pii",
                principalTable: "person_profiles",
                principalColumn: "subject_ref",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_alias_owner_map_person_profile",
                schema: "pii",
                table: "patient_owner_map",
                column: "subject_ref",
                principalSchema: "pii",
                principalTable: "person_profiles",
                principalColumn: "subject_ref",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_alias_owner_map_person_profile",
                schema: "pii",
                table: "alias_owner_map");

            migrationBuilder.DropForeignKey(
                name: "fk_alias_owner_map_person_profile",
                schema: "pii",
                table: "patient_owner_map");

            migrationBuilder.DropPrimaryKey(
                name: "ak_person_profiles_subject_ref",
                schema: "pii",
                table: "person_profiles");

            migrationBuilder.AddPrimaryKey(
                name: "pk_person_profiles_subject_ref",
                schema: "pii",
                table: "person_profiles",
                column: "subject_ref");

            migrationBuilder.AddForeignKey(
                name: "fk_alias_owner_map_person_profiles_subject_ref",
                schema: "pii",
                table: "alias_owner_map",
                column: "subject_ref",
                principalSchema: "pii",
                principalTable: "person_profiles",
                principalColumn: "subject_ref",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_patient_owner_map_person_profiles_subject_ref",
                schema: "pii",
                table: "patient_owner_map",
                column: "subject_ref",
                principalSchema: "pii",
                principalTable: "person_profiles",
                principalColumn: "subject_ref",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
