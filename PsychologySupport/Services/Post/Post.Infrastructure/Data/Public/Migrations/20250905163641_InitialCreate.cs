using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Post.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.CreateTable(
            //     name: "category_tags",
            //     columns: table => new
            //     {
            //         id = table.Column<Guid>(type: "uuid", nullable: false),
            //         code = table.Column<string>(type: "text", nullable: false),
            //         display_name = table.Column<string>(type: "text", nullable: false),
            //         color = table.Column<string>(type: "text", nullable: true),
            //         is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
            //         sort_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
            //         created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
            //         last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
            //         unicode_codepoint = table.Column<string>(type: "text", nullable: true)
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("category_tags_pkey", x => x.id);
            //     });
            //
            // migrationBuilder.CreateTable(
            //     name: "comments",
            //     columns: table => new
            //     {
            //         id = table.Column<Guid>(type: "uuid", nullable: false),
            //         post_id = table.Column<Guid>(type: "uuid", nullable: false),
            //         parent_comment_id = table.Column<Guid>(type: "uuid", nullable: true),
            //         path = table.Column<string>(type: "text", nullable: false),
            //         level = table.Column<int>(type: "integer", nullable: false),
            //         content = table.Column<string>(type: "text", nullable: false),
            //         author_alias_id = table.Column<Guid>(type: "uuid", nullable: false),
            //         author_alias_version_id = table.Column<Guid>(type: "uuid", nullable: false),
            //         moderation_status = table.Column<string>(type: "text", nullable: false, defaultValueSql: "'pending'::text"),
            //         created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
            //         created_by = table.Column<Guid>(type: "uuid", nullable: false),
            //         last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
            //         last_modified_by = table.Column<Guid>(type: "uuid", nullable: true),
            //         deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("comments_pkey", x => x.id);
            //     });
            //
            // migrationBuilder.CreateTable(
            //     name: "emotion_tags",
            //     columns: table => new
            //     {
            //         id = table.Column<Guid>(type: "uuid", nullable: false),
            //         code = table.Column<string>(type: "text", nullable: false),
            //         display_name = table.Column<string>(type: "text", nullable: false),
            //         icon = table.Column<string>(type: "text", nullable: true),
            //         color = table.Column<string>(type: "text", nullable: true),
            //         is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
            //         sort_order = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
            //         created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
            //         last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
            //         digital_good_id = table.Column<Guid>(type: "uuid", nullable: true),
            //         unicode_codepoint = table.Column<string>(type: "text", nullable: true),
            //         media_id = table.Column<Guid>(type: "uuid", nullable: true),
            //         topic = table.Column<string>(type: "text", nullable: true)
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("emotion_tags_pkey", x => x.id);
            //     });
            //
            // migrationBuilder.CreateTable(
            //     name: "gifts_attach",
            //     columns: table => new
            //     {
            //         id = table.Column<Guid>(type: "uuid", nullable: false),
            //         target_type = table.Column<string>(type: "text", nullable: false),
            //         target_id = table.Column<Guid>(type: "uuid", nullable: false),
            //         gift_id = table.Column<Guid>(type: "uuid", nullable: false),
            //         message = table.Column<string>(type: "text", nullable: true),
            //         sender_alias_id = table.Column<Guid>(type: "uuid", nullable: false),
            //         sender_alias_version_id = table.Column<Guid>(type: "uuid", nullable: false),
            //         created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
            //         created_by = table.Column<Guid>(type: "uuid", nullable: false),
            //         deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("gifts_attach_pkey", x => x.id);
            //     });
            //
            // migrationBuilder.CreateTable(
            //     name: "idempotency_keys",
            //     columns: table => new
            //     {
            //         id = table.Column<Guid>(type: "uuid", nullable: false),
            //         idempotency_key = table.Column<Guid>(type: "uuid", nullable: false),
            //         request_fingerprint = table.Column<byte[]>(type: "bytea", nullable: false),
            //         created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("idempotency_keys_pkey", x => x.id);
            //     });
            //
            // migrationBuilder.CreateTable(
            //     name: "outbox_messages",
            //     columns: table => new
            //     {
            //         id = table.Column<Guid>(type: "uuid", nullable: false),
            //         type = table.Column<string>(type: "text", nullable: false),
            //         content = table.Column<string>(type: "text", nullable: false),
            //         occured_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
            //         processed_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
            //         created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
            //         created_by = table.Column<string>(type: "text", nullable: true),
            //         last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
            //         last_modified_by = table.Column<string>(type: "text", nullable: true)
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("outbox_messages_pkey", x => x.id);
            //     });
            //
            // migrationBuilder.CreateTable(
            //     name: "post_categories",
            //     columns: table => new
            //     {
            //         id = table.Column<Guid>(type: "uuid", nullable: false),
            //         post_id = table.Column<Guid>(type: "uuid", nullable: false),
            //         category_tag_id = table.Column<Guid>(type: "uuid", nullable: false),
            //         created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
            //         created_by_alias_id = table.Column<Guid>(type: "uuid", nullable: false),
            //         deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("post_categories_pkey", x => x.id);
            //     });
            //
            // migrationBuilder.CreateTable(
            //     name: "post_counter_deltas",
            //     columns: table => new
            //     {
            //         id = table.Column<Guid>(type: "uuid", nullable: false),
            //         post_id = table.Column<Guid>(type: "uuid", nullable: false),
            //         kind = table.Column<string>(type: "text", nullable: false),
            //         delta = table.Column<short>(type: "smallint", nullable: false),
            //         occured_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
            //         processed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("post_counter_deltas_pkey", x => x.id);
            //     });
            //
            // migrationBuilder.CreateTable(
            //     name: "post_emotions",
            //     columns: table => new
            //     {
            //         id = table.Column<Guid>(type: "uuid", nullable: false),
            //         post_id = table.Column<Guid>(type: "uuid", nullable: false),
            //         emotion_tag_id = table.Column<Guid>(type: "uuid", nullable: false),
            //         created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
            //         created_by_alias_id = table.Column<Guid>(type: "uuid", nullable: false),
            //         deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("post_emotions_pkey", x => x.id);
            //     });
            //
            // migrationBuilder.CreateTable(
            //     name: "post_media",
            //     columns: table => new
            //     {
            //         id = table.Column<Guid>(type: "uuid", nullable: false),
            //         post_id = table.Column<Guid>(type: "uuid", nullable: false),
            //         media_id = table.Column<Guid>(type: "uuid", nullable: false),
            //         position = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
            //         created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
            //         created_by = table.Column<Guid>(type: "uuid", nullable: false),
            //         deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("post_media_pkey", x => x.id);
            //     });
            //
            // migrationBuilder.CreateTable(
            //     name: "posts",
            //     columns: table => new
            //     {
            //         id = table.Column<Guid>(type: "uuid", nullable: false),
            //         visibility = table.Column<string>(type: "text", nullable: false),
            //         title = table.Column<string>(type: "text", nullable: true),
            //         content = table.Column<string>(type: "text", nullable: false),
            //         author_alias_id = table.Column<Guid>(type: "uuid", nullable: false),
            //         author_alias_version_id = table.Column<Guid>(type: "uuid", nullable: false),
            //         moderation_status = table.Column<string>(type: "text", nullable: false, defaultValueSql: "'pending'::text"),
            //         moderation_reasons = table.Column<List<string>>(type: "text[]", nullable: false, defaultValueSql: "'{}'::text[]"),
            //         moderation_policy_version = table.Column<string>(type: "text", nullable: true),
            //         reaction_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
            //         comment_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
            //         created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
            //         created_by = table.Column<Guid>(type: "uuid", nullable: false),
            //         last_modified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
            //         last_modified_by = table.Column<Guid>(type: "uuid", nullable: true),
            //         deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("posts_pkey", x => x.id);
            //     });
            //
            // migrationBuilder.CreateTable(
            //     name: "reactions",
            //     columns: table => new
            //     {
            //         id = table.Column<Guid>(type: "uuid", nullable: false),
            //         target_type = table.Column<string>(type: "text", nullable: false),
            //         target_id = table.Column<Guid>(type: "uuid", nullable: false),
            //         reaction_type = table.Column<string>(type: "text", nullable: false),
            //         author_alias_id = table.Column<Guid>(type: "uuid", nullable: false),
            //         author_alias_version_id = table.Column<Guid>(type: "uuid", nullable: false),
            //         created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
            //         created_by = table.Column<Guid>(type: "uuid", nullable: false),
            //         deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("reactions_pkey", x => x.id);
            //     });
            //
            // migrationBuilder.CreateIndex(
            //     name: "ix_category_tags_code",
            //     table: "category_tags",
            //     column: "code",
            //     unique: true);
            //
            // migrationBuilder.CreateIndex(
            //     name: "ix_comments_post_id_created_at_id",
            //     table: "comments",
            //     columns: new[] { "post_id", "created_at", "id" },
            //     descending: new[] { false, true, true },
            //     filter: "((deleted_at IS NULL) AND (moderation_status = 'approved'::text))");
            //
            // migrationBuilder.CreateIndex(
            //     name: "ix_comments_post_id_path",
            //     table: "comments",
            //     columns: new[] { "post_id", "path" });
            //
            // migrationBuilder.CreateIndex(
            //     name: "ix_emotion_tags_code",
            //     table: "emotion_tags",
            //     column: "code",
            //     unique: true);
            //
            // migrationBuilder.CreateIndex(
            //     name: "ix_gifts_attach_target_type_target_id_created_at",
            //     table: "gifts_attach",
            //     columns: new[] { "target_type", "target_id", "created_at" },
            //     descending: new[] { false, false, true },
            //     filter: "(deleted_at IS NULL)");
            //
            // migrationBuilder.CreateIndex(
            //     name: "ix_idempotency_keys_idempotency_key1",
            //     table: "idempotency_keys",
            //     column: "idempotency_key",
            //     unique: true);
            //
            // migrationBuilder.CreateIndex(
            //     name: "ix_outbox_messages_occured_on",
            //     table: "outbox_messages",
            //     column: "occured_on",
            //     filter: "(processed_on IS NULL)");
            //
            // migrationBuilder.CreateIndex(
            //     name: "ix_outbox_messages_processed_on",
            //     table: "outbox_messages",
            //     column: "processed_on",
            //     filter: "(processed_on IS NOT NULL)");
            //
            // migrationBuilder.CreateIndex(
            //     name: "ix_post_categories_category_tag_id",
            //     table: "post_categories",
            //     column: "category_tag_id",
            //     filter: "(deleted_at IS NULL)");
            //
            // migrationBuilder.CreateIndex(
            //     name: "ix_post_categories_post_id",
            //     table: "post_categories",
            //     column: "post_id",
            //     filter: "(deleted_at IS NULL)");
            //
            // migrationBuilder.CreateIndex(
            //     name: "ix_post_categories_post_id_category_tag_id",
            //     table: "post_categories",
            //     columns: new[] { "post_id", "category_tag_id" },
            //     unique: true,
            //     filter: "(deleted_at IS NULL)");
            //
            // migrationBuilder.CreateIndex(
            //     name: "ix_post_counter_deltas_processed_occured_at",
            //     table: "post_counter_deltas",
            //     columns: new[] { "processed", "occured_at" });
            //
            // migrationBuilder.CreateIndex(
            //     name: "ix_post_emotions_emotion_tag_id",
            //     table: "post_emotions",
            //     column: "emotion_tag_id",
            //     filter: "(deleted_at IS NULL)");
            //
            // migrationBuilder.CreateIndex(
            //     name: "ix_post_emotions_post_id",
            //     table: "post_emotions",
            //     column: "post_id",
            //     filter: "(deleted_at IS NULL)");
            //
            // migrationBuilder.CreateIndex(
            //     name: "ix_post_emotions_post_id_emotion_tag_id",
            //     table: "post_emotions",
            //     columns: new[] { "post_id", "emotion_tag_id" },
            //     unique: true,
            //     filter: "(deleted_at IS NULL)");
            //
            // migrationBuilder.CreateIndex(
            //     name: "ix_post_media_post_id_position",
            //     table: "post_media",
            //     columns: new[] { "post_id", "position" });
            //
            // migrationBuilder.CreateIndex(
            //     name: "ix_posts_author_alias_id_created_at",
            //     table: "posts",
            //     columns: new[] { "author_alias_id", "created_at" },
            //     descending: new[] { false, true },
            //     filter: "(deleted_at IS NULL)");
            //
            // migrationBuilder.CreateIndex(
            //     name: "ix_posts_created_at_id",
            //     table: "posts",
            //     columns: new[] { "created_at", "id" },
            //     descending: new bool[0],
            //     filter: "((deleted_at IS NULL) AND (moderation_status = 'approved'::text))");
            //
            // migrationBuilder.CreateIndex(
            //     name: "ix_reactions_target_type_target_id_author_alias_id_reaction_ty",
            //     table: "reactions",
            //     columns: new[] { "target_type", "target_id", "author_alias_id", "reaction_type" },
            //     unique: true,
            //     filter: "(deleted_at IS NULL)");
            //
            // migrationBuilder.CreateIndex(
            //     name: "ix_reactions_target_type_target_id_created_at",
            //     table: "reactions",
            //     columns: new[] { "target_type", "target_id", "created_at" },
            //     descending: new[] { false, false, true },
            //     filter: "(deleted_at IS NULL)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "category_tags");

            migrationBuilder.DropTable(
                name: "comments");

            migrationBuilder.DropTable(
                name: "emotion_tags");

            migrationBuilder.DropTable(
                name: "gifts_attach");

            migrationBuilder.DropTable(
                name: "idempotency_keys");

            migrationBuilder.DropTable(
                name: "outbox_messages");

            migrationBuilder.DropTable(
                name: "post_categories");

            migrationBuilder.DropTable(
                name: "post_counter_deltas");

            migrationBuilder.DropTable(
                name: "post_emotions");

            migrationBuilder.DropTable(
                name: "post_media");

            migrationBuilder.DropTable(
                name: "posts");

            migrationBuilder.DropTable(
                name: "reactions");
        }
    }
}
