using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChatBox.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSessionSummarization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastSummarizedAt",
                schema: "public",
                table: "AIChatSessions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LastSummarizedIndex",
                schema: "public",
                table: "AIChatSessions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Summarization",
                schema: "public",
                table: "AIChatSessions",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastSummarizedAt",
                schema: "public",
                table: "AIChatSessions");

            migrationBuilder.DropColumn(
                name: "LastSummarizedIndex",
                schema: "public",
                table: "AIChatSessions");

            migrationBuilder.DropColumn(
                name: "Summarization",
                schema: "public",
                table: "AIChatSessions");
        }
    }
}
