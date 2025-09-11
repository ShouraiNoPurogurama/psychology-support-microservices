using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Profile.API.Data.Pii.Migrations
{
    /// <inheritdoc />
    public partial class AddPatientOwnerMapping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "patient_owner_map",
                schema: "pii",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    subject_ref = table.Column<Guid>(type: "uuid", nullable: false),
                    patient_profile_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("patient_owner_map_pkey", x => x.id);
                    table.ForeignKey(
                        name: "fk_patient_owner_map_person_profiles_subject_ref",
                        column: x => x.subject_ref,
                        principalSchema: "pii",
                        principalTable: "person_profiles",
                        principalColumn: "subject_ref",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_patient_owner_map_patient_profile_id",
                schema: "pii",
                table: "patient_owner_map",
                column: "patient_profile_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_patient_owner_map_subject_ref",
                schema: "pii",
                table: "patient_owner_map",
                column: "subject_ref",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "patient_owner_map",
                schema: "pii");
        }
    }
}
