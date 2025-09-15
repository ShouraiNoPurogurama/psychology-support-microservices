using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Profile.API.Data.Pii.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceIdBySubjectRef : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.DropPrimaryKey(
            //     name: "pk_person_profiles",
            //     schema: "pii",
            //     table: "person_profiles");
            //
            // migrationBuilder.AddPrimaryKey(
            //     name: "ak_person_profiles_subject_ref",
            //     schema: "pii",
            //     table: "person_profiles",
            //     column: "subject_ref");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.DropPrimaryKey(
            //     name: "ak_person_profiles_subject_ref",
            //     schema: "pii",
            //     table: "person_profiles");
            //
            // migrationBuilder.AddPrimaryKey(
            //     name: "pk_person_profiles",
            //     schema: "pii",
            //     table: "person_profiles",
            //     column: "subject_ref");
        }
    }
}
