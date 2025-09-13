using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Media.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class Change_MediaOwnerId_StringToGuid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Dùng raw SQL để convert text -> uuid
            migrationBuilder.Sql(@"
                ALTER TABLE media_owners
                ALTER COLUMN media_owner_id TYPE uuid
                USING media_owner_id::uuid;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Convert ngược uuid -> text
            migrationBuilder.Sql(@"
                ALTER TABLE media_owners
                ALTER COLUMN media_owner_id TYPE text
                USING media_owner_id::text;
            ");
        }
    }
}
