using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Post.Infrastructure.Data.Query.Migrations
{
    /// <inheritdoc />
    public partial class AddValidPeriodToUserOwnedTagReplica : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "valid_from",
                schema: "query",
                table: "user_owned_tag_replicas",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "valid_to",
                schema: "query",
                table: "user_owned_tag_replicas",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "valid_from",
                schema: "query",
                table: "user_owned_tag_replicas");

            migrationBuilder.DropColumn(
                name: "valid_to",
                schema: "query",
                table: "user_owned_tag_replicas");
        }
    }
}
