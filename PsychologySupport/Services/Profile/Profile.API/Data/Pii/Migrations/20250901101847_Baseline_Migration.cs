using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Profile.API.Data.Pii.Migrations
{
    /// <inheritdoc />
    public partial class Baseline_Migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "pii");

            // migrationBuilder.AlterDatabase()
            //     .Annotation("Npgsql:PostgresExtension:pii.citext", ",,");
            //
            // migrationBuilder.CreateTable(
            //     name: "person_profiles",
            //     schema: "pii",
            //     columns: table => new
            //     {
            //         subject_ref = table.Column<Guid>(type: "uuid", nullable: false),
            //         user_id = table.Column<Guid>(type: "uuid", nullable: false),
            //         full_name = table.Column<string>(type: "text", nullable: true),
            //         gender = table.Column<string>(type: "text", nullable: false, defaultValue: "Else"),
            //         birth_date = table.Column<DateOnly>(type: "date", nullable: true),
            //         created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, defaultValueSql: "now()"),
            //         created_by = table.Column<string>(type: "text", nullable: true),
            //         last_modified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
            //         last_modified_by = table.Column<string>(type: "text", nullable: true),
            //         address = table.Column<string>(type: "text", nullable: false),
            //         email = table.Column<string>(type: "text", nullable: false),
            //         phone_number = table.Column<string>(type: "text", nullable: true)
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("pk_person_profiles", x => x.subject_ref);
            //     });
            //
            // migrationBuilder.CreateTable(
            //     name: "alias_owner_map",
            //     schema: "pii",
            //     columns: table => new
            //     {
            //         id = table.Column<Guid>(type: "uuid", nullable: false),
            //         alias_id = table.Column<Guid>(type: "uuid", nullable: false),
            //         subject_ref = table.Column<Guid>(type: "uuid", nullable: false),
            //         created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
            //         created_by = table.Column<string>(type: "text", nullable: true)
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("alias_owner_map_pkey", x => x.id);
            //         table.ForeignKey(
            //             name: "fk_alias_owner_map_person_profiles_subject_ref",
            //             column: x => x.subject_ref,
            //             principalSchema: "pii",
            //             principalTable: "person_profiles",
            //             principalColumn: "subject_ref",
            //             onDelete: ReferentialAction.Cascade);
            //     });
            //
            // migrationBuilder.CreateIndex(
            //     name: "ix_alias_owner_map_alias_id",
            //     schema: "pii",
            //     table: "alias_owner_map",
            //     column: "alias_id",
            //     unique: true);
            //
            // migrationBuilder.CreateIndex(
            //     name: "ix_alias_owner_map_subject_ref",
            //     schema: "pii",
            //     table: "alias_owner_map",
            //     column: "subject_ref",
            //     unique: true);
            //
            // migrationBuilder.CreateIndex(
            //     name: "ix_person_profiles_user_id",
            //     schema: "pii",
            //     table: "person_profiles",
            //     column: "user_id",
            //     unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "alias_owner_map",
                schema: "pii");

            migrationBuilder.DropTable(
                name: "person_profiles",
                schema: "pii");
        }
    }
}
