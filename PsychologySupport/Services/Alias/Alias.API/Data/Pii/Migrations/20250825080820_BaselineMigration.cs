using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Alias.API.Data.Pii.Migrations
{
    /// <inheritdoc />
    public partial class BaselineMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "pii");

            migrationBuilder.CreateTable(
                name: "alias_owner_map",
                schema: "pii",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    alias_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("alias_owner_map_pkey", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_alias_owner_map_alias_id",
                schema: "pii",
                table: "alias_owner_map",
                column: "alias_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_alias_owner_map_user_id",
                schema: "pii",
                table: "alias_owner_map",
                column: "user_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "alias_owner_map",
                schema: "pii");
        }
    }
}
