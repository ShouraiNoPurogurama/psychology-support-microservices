using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Post.Infrastructure.Data.Post.Migrations
{
    /// <inheritdoc />
    public partial class UpdateModerationMechanism : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "moderated_at",
                schema: "post",
                table: "posts",
                newName: "evaluated_at");

            migrationBuilder.RenameColumn(
                name: "moderated_at",
                schema: "post",
                table: "comments",
                newName: "evaluated_at");

            migrationBuilder.AlterColumn<List<string>>(
                name: "moderation_reasons",
                schema: "post",
                table: "posts",
                type: "text[]",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<List<string>>(
                name: "moderation_reasons",
                schema: "post",
                table: "comments",
                type: "text[]",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "evaluated_at",
                schema: "post",
                table: "posts",
                newName: "moderated_at");

            migrationBuilder.RenameColumn(
                name: "evaluated_at",
                schema: "post",
                table: "comments",
                newName: "moderated_at");

            migrationBuilder.AlterColumn<string>(
                name: "moderation_reasons",
                schema: "post",
                table: "posts",
                type: "text",
                nullable: false,
                oldClrType: typeof(List<string>),
                oldType: "text[]");

            migrationBuilder.AlterColumn<string>(
                name: "moderation_reasons",
                schema: "post",
                table: "comments",
                type: "text",
                nullable: false,
                oldClrType: typeof(List<string>),
                oldType: "text[]");
        }
    }
}
