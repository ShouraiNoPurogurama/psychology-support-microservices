using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wellness.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateChallenge : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_article_progresses_module_progresses_module_progress_id",
                table: "article_progresses");

            migrationBuilder.DropForeignKey(
                name: "fk_article_progresses_section_articles_article_id",
                table: "article_progresses");

            migrationBuilder.DropForeignKey(
                name: "fk_challenge_progresses_challenges_challenge_id",
                table: "challenge_progresses");

            migrationBuilder.DropForeignKey(
                name: "fk_challenge_step_progresses_challenge_progresses_challenge_pr",
                table: "challenge_step_progresses");

            migrationBuilder.DropForeignKey(
                name: "fk_challenge_step_progresses_challenge_steps_challenge_step_id",
                table: "challenge_step_progresses");

            migrationBuilder.DropForeignKey(
                name: "fk_challenge_steps_activities_activity_id",
                table: "challenge_steps");

            migrationBuilder.DropColumn(
                name: "module_id",
                table: "challenges");

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "module_sections",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "activity_id",
                table: "challenge_steps",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "challenge_step_id",
                table: "challenge_step_progresses",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "challenge_progress_id",
                table: "challenge_step_progresses",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "progress_percent",
                table: "challenge_progresses",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "challenge_id",
                table: "challenge_progresses",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "module_progress_id",
                table: "article_progresses",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "article_id",
                table: "article_progresses",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "fk_article_progresses_module_progresses_module_progress_id",
                table: "article_progresses",
                column: "module_progress_id",
                principalTable: "module_progresses",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_article_progresses_section_articles_article_id",
                table: "article_progresses",
                column: "article_id",
                principalTable: "section_articles",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_challenge_progresses_challenges_challenge_id",
                table: "challenge_progresses",
                column: "challenge_id",
                principalTable: "challenges",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_challenge_step_progresses_challenge_progresses_challenge_pr",
                table: "challenge_step_progresses",
                column: "challenge_progress_id",
                principalTable: "challenge_progresses",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_challenge_step_progresses_challenge_steps_challenge_step_id",
                table: "challenge_step_progresses",
                column: "challenge_step_id",
                principalTable: "challenge_steps",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_challenge_steps_activities_activity_id",
                table: "challenge_steps",
                column: "activity_id",
                principalTable: "activities",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_article_progresses_module_progresses_module_progress_id",
                table: "article_progresses");

            migrationBuilder.DropForeignKey(
                name: "fk_article_progresses_section_articles_article_id",
                table: "article_progresses");

            migrationBuilder.DropForeignKey(
                name: "fk_challenge_progresses_challenges_challenge_id",
                table: "challenge_progresses");

            migrationBuilder.DropForeignKey(
                name: "fk_challenge_step_progresses_challenge_progresses_challenge_pr",
                table: "challenge_step_progresses");

            migrationBuilder.DropForeignKey(
                name: "fk_challenge_step_progresses_challenge_steps_challenge_step_id",
                table: "challenge_step_progresses");

            migrationBuilder.DropForeignKey(
                name: "fk_challenge_steps_activities_activity_id",
                table: "challenge_steps");

            migrationBuilder.AlterColumn<string>(
                name: "description",
                table: "module_sections",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<Guid>(
                name: "module_id",
                table: "challenges",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "activity_id",
                table: "challenge_steps",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "challenge_step_id",
                table: "challenge_step_progresses",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "challenge_progress_id",
                table: "challenge_step_progresses",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<int>(
                name: "progress_percent",
                table: "challenge_progresses",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<Guid>(
                name: "challenge_id",
                table: "challenge_progresses",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "module_progress_id",
                table: "article_progresses",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "article_id",
                table: "article_progresses",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "fk_article_progresses_module_progresses_module_progress_id",
                table: "article_progresses",
                column: "module_progress_id",
                principalTable: "module_progresses",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_article_progresses_section_articles_article_id",
                table: "article_progresses",
                column: "article_id",
                principalTable: "section_articles",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_challenge_progresses_challenges_challenge_id",
                table: "challenge_progresses",
                column: "challenge_id",
                principalTable: "challenges",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_challenge_step_progresses_challenge_progresses_challenge_pr",
                table: "challenge_step_progresses",
                column: "challenge_progress_id",
                principalTable: "challenge_progresses",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_challenge_step_progresses_challenge_steps_challenge_step_id",
                table: "challenge_step_progresses",
                column: "challenge_step_id",
                principalTable: "challenge_steps",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_challenge_steps_activities_activity_id",
                table: "challenge_steps",
                column: "activity_id",
                principalTable: "activities",
                principalColumn: "id");
        }
    }
}
