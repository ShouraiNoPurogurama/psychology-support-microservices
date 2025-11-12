using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatBox.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOnboardingCompletionProps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_onboarding_completed",
                schema: "public",
                table: "ai_chat_sessions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "onboarding_step",
                schema: "public",
                table: "ai_chat_sessions",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_onboarding_completed",
                schema: "public",
                table: "ai_chat_sessions");

            migrationBuilder.DropColumn(
                name: "onboarding_step",
                schema: "public",
                table: "ai_chat_sessions");
        }
    }
}
