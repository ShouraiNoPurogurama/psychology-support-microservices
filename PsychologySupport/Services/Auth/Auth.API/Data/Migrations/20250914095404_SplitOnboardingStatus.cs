using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auth.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class SplitOnboardingStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "onboarding_status",
                table: "AspNetUsers");

            migrationBuilder.CreateTable(
                name: "user_onboarding",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    pii_completed = table.Column<bool>(type: "boolean", nullable: false),
                    profile_completed = table.Column<bool>(type: "boolean", nullable: false),
                    missing = table.Column<string>(type: "jsonb", nullable: false),
                    reason_code = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    last_modified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_modified_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_onboarding", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_onboarding_user_user_id",
                        column: x => x.user_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_user_onboarding_user_id",
                table: "user_onboarding",
                column: "user_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_onboarding");

            migrationBuilder.AddColumn<string>(
                name: "onboarding_status",
                table: "AspNetUsers",
                type: "text",
                nullable: false,
                defaultValue: "Pending");
        }
    }
}
