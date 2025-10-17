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

            // --- POSTS ---
            // 1) Thêm cột tạm kiểu text[]
            migrationBuilder.Sql(@"
ALTER TABLE post.posts
ADD COLUMN moderation_reasons_tmp text[] DEFAULT ARRAY[]::text[] NOT NULL;
");

            // 2) Đổ dữ liệu sang cột tạm (cho phép subquery trong UPDATE)
            migrationBuilder.Sql(@"
UPDATE post.posts
SET moderation_reasons_tmp =
  CASE
    WHEN moderation_reasons IS NULL OR btrim(moderation_reasons) = '' THEN ARRAY[]::text[]
    WHEN moderation_reasons ~ '^\s*\[' THEN
      COALESCE(
        ARRAY(SELECT value FROM jsonb_array_elements_text(moderation_reasons::jsonb)),
        ARRAY[]::text[]
      )
    ELSE
      string_to_array(moderation_reasons, ',')
  END;
");

            // 3) Bỏ cột cũ và đổi tên cột tạm
            migrationBuilder.Sql(@"
ALTER TABLE post.posts DROP COLUMN moderation_reasons;
ALTER TABLE post.posts RENAME COLUMN moderation_reasons_tmp TO moderation_reasons;
");

            // --- COMMENTS ---
            migrationBuilder.Sql(@"
ALTER TABLE post.comments
ADD COLUMN moderation_reasons_tmp text[] DEFAULT ARRAY[]::text[] NOT NULL;
");

            migrationBuilder.Sql(@"
UPDATE post.comments
SET moderation_reasons_tmp =
  CASE
    WHEN moderation_reasons IS NULL OR btrim(moderation_reasons) = '' THEN ARRAY[]::text[]
    WHEN moderation_reasons ~ '^\s*\[' THEN
      COALESCE(
        ARRAY(SELECT value FROM jsonb_array_elements_text(moderation_reasons::jsonb)),
        ARRAY[]::text[]
      )
    ELSE
      string_to_array(moderation_reasons, ',')
  END;
");

            migrationBuilder.Sql(@"
ALTER TABLE post.comments DROP COLUMN moderation_reasons;
ALTER TABLE post.comments RENAME COLUMN moderation_reasons_tmp TO moderation_reasons;
");

            
            // migrationBuilder.AlterColumn<List<string>>(
            //     name: "moderation_reasons",
            //     schema: "post",
            //     table: "posts",
            //     type: "text[]",
            //     nullable: false,
            //     oldClrType: typeof(string),
            //     oldType: "text");
            //
            // migrationBuilder.AlterColumn<List<string>>(
            //     name: "moderation_reasons",
            //     schema: "post",
            //     table: "comments",
            //     type: "text[]",
            //     nullable: false,
            //     oldClrType: typeof(string),
            //     oldType: "text");
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

            migrationBuilder.Sql(@"
ALTER TABLE post.posts
ALTER COLUMN moderation_reasons
TYPE text
USING array_to_string(COALESCE(moderation_reasons, ARRAY[]::text[]), ',');
ALTER TABLE post.posts
ALTER COLUMN moderation_reasons SET NOT NULL;
");

            // === COMMENTS: text[] -> text (CSV) ===
            migrationBuilder.Sql(@"
ALTER TABLE post.comments
ALTER COLUMN moderation_reasons
TYPE text
USING array_to_string(COALESCE(moderation_reasons, ARRAY[]::text[]), ',');
ALTER TABLE post.comments
ALTER COLUMN moderation_reasons SET NOT NULL;
");
            
            // migrationBuilder.AlterColumn<string>(
            //     name: "moderation_reasons",
            //     schema: "post",
            //     table: "posts",
            //     type: "text",
            //     nullable: false,
            //     oldClrType: typeof(List<string>),
            //     oldType: "text[]");
            //
            // migrationBuilder.AlterColumn<string>(
            //     name: "moderation_reasons",
            //     schema: "post",
            //     table: "comments",
            //     type: "text",
            //     nullable: false,
            //     oldClrType: typeof(List<string>),
            //     oldType: "text[]");
        }
    }
}
