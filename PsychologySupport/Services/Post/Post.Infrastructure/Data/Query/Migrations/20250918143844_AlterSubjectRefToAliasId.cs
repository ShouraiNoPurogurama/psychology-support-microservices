using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Post.Infrastructure.Data.Query.Migrations
{
    /// <inheritdoc />
    public partial class AlterSubjectRefToAliasId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "subject_ref",
                schema: "query",
                table: "user_owned_tag_replicas",
                newName: "alias_id");

            migrationBuilder.AddColumn<string>(
                name: "avatar_url",
                schema: "query",
                table: "alias_version_replica",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "avatar_url",
                schema: "query",
                table: "alias_version_replica");

            migrationBuilder.RenameColumn(
                name: "alias_id",
                schema: "query",
                table: "user_owned_tag_replicas",
                newName: "subject_ref");
        }
    }
}
