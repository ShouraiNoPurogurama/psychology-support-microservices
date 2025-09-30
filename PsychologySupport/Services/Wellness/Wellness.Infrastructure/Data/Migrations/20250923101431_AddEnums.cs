using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wellness.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEnums : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "difficulty",
                table: "challenges");

            migrationBuilder.RenameColumn(
                name: "process_status",
                table: "process_histories",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "process_status",
                table: "module_progresses",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "challenge_type",
                table: "challenges",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "process_status",
                table: "challenge_step_progresses",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "process_status",
                table: "challenge_progresses",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "process_status",
                table: "article_progresses",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "activity_type",
                table: "activities",
                newName: "status");

            migrationBuilder.AlterColumn<int>(
                name: "duration",
                table: "section_articles",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "process_histories",
                type: "VARCHAR(20)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "total_duration",
                table: "module_sections",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "module_progresses",
                type: "VARCHAR(20)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "challenges",
                type: "VARCHAR(20)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "challenge_step_progresses",
                type: "VARCHAR(20)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "challenge_progresses",
                type: "VARCHAR(20)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "article_progresses",
                type: "VARCHAR(20)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "activities",
                type: "VARCHAR(20)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "status",
                table: "process_histories",
                newName: "process_status");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "module_progresses",
                newName: "process_status");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "challenges",
                newName: "challenge_type");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "challenge_step_progresses",
                newName: "process_status");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "challenge_progresses",
                newName: "process_status");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "article_progresses",
                newName: "process_status");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "activities",
                newName: "activity_type");

            migrationBuilder.AlterColumn<int>(
                name: "duration",
                table: "section_articles",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "process_status",
                table: "process_histories",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "VARCHAR(20)");

            migrationBuilder.AlterColumn<int>(
                name: "total_duration",
                table: "module_sections",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "process_status",
                table: "module_progresses",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "VARCHAR(20)");

            migrationBuilder.AlterColumn<string>(
                name: "challenge_type",
                table: "challenges",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "VARCHAR(20)");

            migrationBuilder.AddColumn<string>(
                name: "difficulty",
                table: "challenges",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "process_status",
                table: "challenge_step_progresses",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "VARCHAR(20)");

            migrationBuilder.AlterColumn<string>(
                name: "process_status",
                table: "challenge_progresses",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "VARCHAR(20)");

            migrationBuilder.AlterColumn<string>(
                name: "process_status",
                table: "article_progresses",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "VARCHAR(20)");

            migrationBuilder.AlterColumn<string>(
                name: "activity_type",
                table: "activities",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "VARCHAR(20)");
        }
    }
}
