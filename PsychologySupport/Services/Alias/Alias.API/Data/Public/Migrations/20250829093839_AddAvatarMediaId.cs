using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Alias.API.Data.Public.Migrations
{
    /// <inheritdoc />
    public partial class AddAvatarMediaId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "avatar_media_id",
                table: "aliases",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "avatar_media_id",
                table: "aliases");
        }
    }
}
