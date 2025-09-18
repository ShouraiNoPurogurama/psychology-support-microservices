using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Profile.API.Data.Pii.Migrations
{
    /// <inheritdoc />
    public partial class CorrectFK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.DropPrimaryKey(
            //     name: "ak_person_profiles_subject_ref",
            //     schema: "pii",
            //     table: "person_profiles");

            migrationBuilder.DropIndex(
                name: "ix_patient_owner_map_subject_ref",
                schema: "pii",
                table: "patient_owner_map");

            migrationBuilder.DropIndex(
                name: "ix_alias_owner_map_subject_ref",
                schema: "pii",
                table: "alias_owner_map");

            // migrationBuilder.AddPrimaryKey(
            //     name: "pk_person_profiles",
            //     schema: "pii",
            //     table: "person_profiles",
            //     column: "subject_ref");

            migrationBuilder.CreateIndex(
                name: "ix_patient_owner_map_subject_ref",
                schema: "pii",
                table: "patient_owner_map",
                column: "subject_ref",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_alias_owner_map_subject_ref",
                schema: "pii",
                table: "alias_owner_map",
                column: "subject_ref",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.DropPrimaryKey(
            //     name: "pk_person_profiles",
            //     schema: "pii",
            //     table: "person_profiles");

            migrationBuilder.DropIndex(
                name: "ix_patient_owner_map_subject_ref",
                schema: "pii",
                table: "patient_owner_map");

            migrationBuilder.DropIndex(
                name: "ix_alias_owner_map_subject_ref",
                schema: "pii",
                table: "alias_owner_map");

            // migrationBuilder.AddPrimaryKey(
            //     name: "ak_person_profiles_subject_ref",
            //     schema: "pii",
            //     table: "person_profiles",
            //     column: "subject_ref");

            migrationBuilder.CreateIndex(
                name: "ix_patient_owner_map_subject_ref",
                schema: "pii",
                table: "patient_owner_map",
                column: "subject_ref");

            migrationBuilder.CreateIndex(
                name: "ix_alias_owner_map_subject_ref",
                schema: "pii",
                table: "alias_owner_map",
                column: "subject_ref");
        }
    }
}
