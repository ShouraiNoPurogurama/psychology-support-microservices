using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Post.Infrastructure.Data.Post.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndexToReactionsManually : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
            CREATE UNIQUE INDEX ""ix_reactions_author_target_unique""
            ON ""post"".""reactions"" (""author_alias_id"", ""target_type"", ""target_id"")
            WHERE ""is_deleted"" = false;
        ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
            DROP INDEX ""post"".""ix_reactions_author_target_unique"";
        ");
        }
    }
}
