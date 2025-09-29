using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Post.Infrastructure.Data.Post.Migrations
{
    /// <inheritdoc />
    public partial class AlterLastModifiedToUUID : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var tables = new[] { "reactions", "posts", "gift_attaches", "comments", "category_tags" };

            foreach (var table in tables)
            {
                migrationBuilder.Sql($@"
            ALTER TABLE post.""{table}""
            ALTER COLUMN last_modified_by_alias_id TYPE uuid
            USING last_modified_by_alias_id::uuid;
        ");
            }            
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var tables = new[] { "reactions", "posts", "gift_attaches", "comments", "category_tags" };

            foreach (var table in tables)
            {
                // Khi downgrade, chúng ta cũng chỉ rõ cách cast ngược lại từ uuid sang text
                migrationBuilder.Sql($@"
            ALTER TABLE post.""{table}""
            ALTER COLUMN last_modified_by_alias_id TYPE text
            USING last_modified_by_alias_id::text;
        ");
            }
        }
    }
}
