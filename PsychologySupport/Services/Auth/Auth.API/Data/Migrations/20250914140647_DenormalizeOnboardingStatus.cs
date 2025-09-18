using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auth.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class DenormalizeOnboardingStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_user_onboarding_user_user_id",
                table: "user_onboarding");

            migrationBuilder.DropPrimaryKey(
                name: "pk_user_onboarding",
                table: "user_onboarding");

            migrationBuilder.RenameTable(
                name: "user_onboarding",
                newName: "user_onboardings");

            migrationBuilder.RenameIndex(
                name: "ix_user_onboarding_user_id",
                table: "user_onboardings",
                newName: "ix_user_onboardings_user_id");

            migrationBuilder.AddColumn<string>(
                name: "onboarding_status",
                table: "AspNetUsers",
                type: "text",
                nullable: false,
                defaultValue: "Pending");

            migrationBuilder.AddPrimaryKey(
                name: "pk_user_onboardings",
                table: "user_onboardings",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_user_onboardings_user_user_id",
                table: "user_onboardings",
                column: "user_id",
                principalTable: "AspNetUsers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_user_onboardings_user_user_id",
                table: "user_onboardings");

            migrationBuilder.DropPrimaryKey(
                name: "pk_user_onboardings",
                table: "user_onboardings");

            migrationBuilder.DropColumn(
                name: "onboarding_status",
                table: "AspNetUsers");

            migrationBuilder.RenameTable(
                name: "user_onboardings",
                newName: "user_onboarding");

            migrationBuilder.RenameIndex(
                name: "ix_user_onboardings_user_id",
                table: "user_onboarding",
                newName: "ix_user_onboarding_user_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_user_onboarding",
                table: "user_onboarding",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_user_onboarding_user_user_id",
                table: "user_onboarding",
                column: "user_id",
                principalTable: "AspNetUsers",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
