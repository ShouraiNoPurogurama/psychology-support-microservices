using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Profile.API.Data.Pii.Migrations
{
    /// <inheritdoc />
    public partial class BaselineMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.EnsureSchema(
            //     name: "pii");
            //
            // migrationBuilder.AlterDatabase()
            //     .Annotation("Npgsql:PostgresExtension:pii.citext", ",,");
            //
            // migrationBuilder.CreateTable(
            //     name: "alias_owner_map",
            //     schema: "pii",
            //     columns: table => new
            //     {
            //         id = table.Column<Guid>(type: "uuid", nullable: false),
            //         alias_id = table.Column<Guid>(type: "uuid", nullable: false),
            //         user_id = table.Column<Guid>(type: "uuid", nullable: false),
            //         created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
            //         created_by = table.Column<string>(type: "text", nullable: true)
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("alias_owner_map_pkey", x => x.id);
            //     });
            //
            // migrationBuilder.CreateTable(
            //     name: "person_profiles",
            //     schema: "pii",
            //     columns: table => new
            //     {
            //         user_id = table.Column<Guid>(type: "uuid", nullable: false),
            //         legal_name = table.Column<string>(type: "text", nullable: true),
            //         gender = table.Column<string>(type: "text", nullable: true),
            //         birth_date = table.Column<DateOnly>(type: "date", nullable: true),
            //         primary_phone = table.Column<string>(type: "text", nullable: true),
            //         current_address = table.Column<string>(type: "text", nullable: true),
            //         created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, defaultValueSql: "now()"),
            //         created_by = table.Column<string>(type: "text", nullable: true),
            //         last_modified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
            //         last_modified_by = table.Column<string>(type: "text", nullable: true)
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("person_profiles_pkey", x => x.user_id);
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
            //     name: "ix_alias_owner_map_user_id",
            //     schema: "pii",
            //     table: "alias_owner_map",
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
