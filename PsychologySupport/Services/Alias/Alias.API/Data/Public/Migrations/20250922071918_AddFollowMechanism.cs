using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Alias.API.Data.Public.Migrations
{
    /// <inheritdoc />
    public partial class AddFollowMechanism : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "followers_count",
                table: "aliases",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "following_count",
                table: "aliases",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "follows",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    follower_alias_id = table.Column<Guid>(type: "uuid", nullable: false),
                    followed_alias_id = table.Column<Guid>(type: "uuid", nullable: false),
                    followed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    last_modified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_modified_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("follows_pkey", x => x.id);
                    table.ForeignKey(
                        name: "fk_follows_followed_alias",
                        column: x => x.followed_alias_id,
                        principalTable: "aliases",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_follows_follower_alias",
                        column: x => x.follower_alias_id,
                        principalTable: "aliases",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_follows_followed_alias_id",
                table: "follows",
                column: "followed_alias_id");

            migrationBuilder.CreateIndex(
                name: "ix_follows_follower_alias_id",
                table: "follows",
                column: "follower_alias_id");

            migrationBuilder.CreateIndex(
                name: "uix_follows_follower_followed",
                table: "follows",
                columns: new[] { "follower_alias_id", "followed_alias_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "follows");

            migrationBuilder.DropColumn(
                name: "followers_count",
                table: "aliases");

            migrationBuilder.DropColumn(
                name: "following_count",
                table: "aliases");
        }
    }
}
