using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auth.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class DenormalizeAliasIssueState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "status",
                table: "user_onboardings");

            migrationBuilder.AddColumn<string>(
                name: "alias_issue_status",
                table: "user_onboardings",
                type: "text",
                nullable: false,
                defaultValue: "Pending");

            migrationBuilder.AddColumn<bool>(
                name: "alias_issued",
                table: "user_onboardings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "onboarding_status",
                table: "user_onboardings",
                type: "text",
                nullable: false,
                defaultValue: "Pending");

            migrationBuilder.AddColumn<string>(
                name: "alias_issue_status",
                table: "AspNetUsers",
                type: "text",
                nullable: false,
                defaultValue: "Pending");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "alias_issue_status",
                table: "user_onboardings");

            migrationBuilder.DropColumn(
                name: "alias_issued",
                table: "user_onboardings");

            migrationBuilder.DropColumn(
                name: "onboarding_status",
                table: "user_onboardings");

            migrationBuilder.DropColumn(
                name: "alias_issue_status",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<string>(
                name: "status",
                table: "user_onboardings",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
