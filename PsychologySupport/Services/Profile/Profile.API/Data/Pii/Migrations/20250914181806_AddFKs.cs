using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Profile.API.Data.Pii.Migrations
{
    /// <inheritdoc />
    public partial class AddFKs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_patient_owner_map_subject_ref",
                schema: "pii",
                table: "patient_owner_map");
            //
            // migrationBuilder.DropIndex(
            //     name: "ix_alias_owner_map_subject_ref",
            //     schema: "pii",
            //     table: "alias_owner_map");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_patient_owner_map_subject_ref",
                schema: "pii",
                table: "patient_owner_map");
            
            // migrationBuilder.DropIndex(
            //     name: "ix_alias_owner_map_subject_ref",
            //     schema: "pii",
            //     table: "alias_owner_map");

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
    }
}
