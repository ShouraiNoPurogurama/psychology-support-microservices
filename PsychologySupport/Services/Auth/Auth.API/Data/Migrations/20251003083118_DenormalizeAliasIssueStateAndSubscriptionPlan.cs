using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auth.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class DenormalizeAliasIssueStateAndSubscriptionPlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "status",
                table: "user_onboardings",
                newName: "onboarding_status");

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
                name: "alias_issue_status",
                table: "AspNetUsers",
                type: "text",
                nullable: false,
                defaultValue: "Pending");

            migrationBuilder.AddColumn<string>(
                name: "subscription_plan_name",
                table: "AspNetUsers",
                type: "text",
                nullable: false,
                defaultValue: "Free Plan");
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
                name: "alias_issue_status",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "subscription_plan_name",
                table: "AspNetUsers");

            migrationBuilder.RenameColumn(
                name: "onboarding_status",
                table: "user_onboardings",
                newName: "status");
        }
    }
}
