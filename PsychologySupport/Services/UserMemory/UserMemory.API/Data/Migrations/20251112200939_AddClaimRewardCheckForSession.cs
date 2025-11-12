using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserMemory.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddClaimRewardCheckForSession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "received_reward_today",
                table: "session_daily_progresses",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "received_reward_today",
                table: "session_daily_progresses");
        }
    }
}
