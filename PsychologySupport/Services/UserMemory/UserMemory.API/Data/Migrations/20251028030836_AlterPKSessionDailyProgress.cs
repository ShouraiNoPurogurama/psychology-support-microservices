using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserMemory.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AlterPKSessionDailyProgress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "pk_session_daily_progresses",
                table: "session_daily_progresses");

            migrationBuilder.AddPrimaryKey(
                name: "pk_session_daily_progresses",
                table: "session_daily_progresses",
                column: "session_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "pk_session_daily_progresses",
                table: "session_daily_progresses");

            migrationBuilder.AddPrimaryKey(
                name: "pk_session_daily_progresses",
                table: "session_daily_progresses",
                columns: new[] { "alias_id", "progress_date" });
        }
    }
}
