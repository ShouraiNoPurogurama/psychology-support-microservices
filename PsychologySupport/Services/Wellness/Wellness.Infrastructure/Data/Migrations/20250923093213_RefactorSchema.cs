using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wellness.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RefactorSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "activities",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    activity_type = table.Column<string>(type: "text", nullable: false),
                    duration = table.Column<int>(type: "integer", nullable: true),
                    instructions = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    last_modified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_modified_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_activities", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "challenges",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    difficulty = table.Column<string>(type: "text", nullable: false),
                    challenge_type = table.Column<string>(type: "text", nullable: false),
                    duration_activity = table.Column<int>(type: "integer", nullable: false),
                    duration_date = table.Column<int>(type: "integer", nullable: false),
                    module_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    last_modified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_modified_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_challenges", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "idempotency_keys",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    idempotency_key1 = table.Column<string>(type: "text", nullable: false),
                    request_hash = table.Column<string>(type: "text", nullable: false),
                    response_payload = table.Column<string>(type: "text", nullable: true),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_idempotency_keys", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "moods",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    icon_code = table.Column<string>(type: "text", nullable: true),
                    value = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_moods", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "outbox_messages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    aggregate_type = table.Column<string>(type: "text", nullable: false),
                    aggregate_id = table.Column<Guid>(type: "uuid", nullable: false),
                    event_type = table.Column<string>(type: "text", nullable: false),
                    payload = table.Column<string>(type: "text", nullable: false),
                    occurred_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    processed_on = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_outbox_messages", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "wellness_modules",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    media_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    last_modified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_modified_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_wellness_modules", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "challenge_progresses",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    subject_ref = table.Column<Guid>(type: "uuid", nullable: false),
                    challenge_id = table.Column<Guid>(type: "uuid", nullable: true),
                    process_status = table.Column<string>(type: "text", nullable: false),
                    progress_percent = table.Column<int>(type: "integer", nullable: true),
                    start_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    end_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    last_modified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_modified_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_challenge_progresses", x => x.id);
                    table.ForeignKey(
                        name: "fk_challenge_progresses_challenges_challenge_id",
                        column: x => x.challenge_id,
                        principalTable: "challenges",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "challenge_steps",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    challenge_id = table.Column<Guid>(type: "uuid", nullable: true),
                    activity_id = table.Column<Guid>(type: "uuid", nullable: true),
                    day_number = table.Column<int>(type: "integer", nullable: false),
                    order_index = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    last_modified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_modified_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_challenge_steps", x => x.id);
                    table.ForeignKey(
                        name: "fk_challenge_steps_activities_activity_id",
                        column: x => x.activity_id,
                        principalTable: "activities",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_challenge_steps_challenges_challenge_id",
                        column: x => x.challenge_id,
                        principalTable: "challenges",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "journal_moods",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    subject_ref = table.Column<Guid>(type: "uuid", nullable: false),
                    mood_id = table.Column<Guid>(type: "uuid", nullable: false),
                    note = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    last_modified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_modified_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_journal_moods", x => x.id);
                    table.ForeignKey(
                        name: "fk_journal_moods_moods_mood_id",
                        column: x => x.mood_id,
                        principalTable: "moods",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "process_histories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    subject_ref = table.Column<Guid>(type: "uuid", nullable: false),
                    activity_id = table.Column<Guid>(type: "uuid", nullable: true),
                    start_time = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    end_time = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    process_status = table.Column<string>(type: "text", nullable: false),
                    post_mood_id = table.Column<Guid>(type: "uuid", nullable: true),
                    challenge_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    last_modified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_modified_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_process_histories", x => x.id);
                    table.ForeignKey(
                        name: "fk_process_histories_activities_activity_id",
                        column: x => x.activity_id,
                        principalTable: "activities",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_process_histories_challenges_challenge_id",
                        column: x => x.challenge_id,
                        principalTable: "challenges",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_process_histories_moods_post_mood_id",
                        column: x => x.post_mood_id,
                        principalTable: "moods",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "module_sections",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    module_id = table.Column<Guid>(type: "uuid", nullable: true),
                    title = table.Column<string>(type: "text", nullable: false),
                    media_id = table.Column<Guid>(type: "uuid", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    total_duration = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    last_modified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_modified_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_module_sections", x => x.id);
                    table.ForeignKey(
                        name: "fk_module_sections_wellness_modules_module_id",
                        column: x => x.module_id,
                        principalTable: "wellness_modules",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "challenge_step_progresses",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    challenge_progress_id = table.Column<Guid>(type: "uuid", nullable: true),
                    challenge_step_id = table.Column<Guid>(type: "uuid", nullable: true),
                    process_status = table.Column<string>(type: "text", nullable: false),
                    started_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    completed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    post_mood_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    last_modified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_modified_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_challenge_step_progresses", x => x.id);
                    table.ForeignKey(
                        name: "fk_challenge_step_progresses_challenge_progresses_challenge_pr",
                        column: x => x.challenge_progress_id,
                        principalTable: "challenge_progresses",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_challenge_step_progresses_challenge_steps_challenge_step_id",
                        column: x => x.challenge_step_id,
                        principalTable: "challenge_steps",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_challenge_step_progresses_moods_post_mood_id",
                        column: x => x.post_mood_id,
                        principalTable: "moods",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "module_progresses",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    subject_ref = table.Column<Guid>(type: "uuid", nullable: false),
                    section_id = table.Column<Guid>(type: "uuid", nullable: false),
                    process_status = table.Column<string>(type: "text", nullable: false),
                    minutes_read = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    last_modified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_modified_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_module_progresses", x => x.id);
                    table.ForeignKey(
                        name: "fk_module_progresses_module_sections_section_id",
                        column: x => x.section_id,
                        principalTable: "module_sections",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "section_articles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    section_id = table.Column<Guid>(type: "uuid", nullable: true),
                    title = table.Column<string>(type: "text", nullable: false),
                    media_id = table.Column<Guid>(type: "uuid", nullable: false),
                    content_json = table.Column<string>(type: "text", nullable: false),
                    order_index = table.Column<int>(type: "integer", nullable: false),
                    duration = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    last_modified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_modified_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_section_articles", x => x.id);
                    table.ForeignKey(
                        name: "fk_section_articles_module_sections_section_id",
                        column: x => x.section_id,
                        principalTable: "module_sections",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "article_progresses",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    module_progress_id = table.Column<Guid>(type: "uuid", nullable: true),
                    article_id = table.Column<Guid>(type: "uuid", nullable: true),
                    process_status = table.Column<string>(type: "text", nullable: false),
                    log_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    last_modified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_modified_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_article_progresses", x => x.id);
                    table.ForeignKey(
                        name: "fk_article_progresses_module_progresses_module_progress_id",
                        column: x => x.module_progress_id,
                        principalTable: "module_progresses",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_article_progresses_section_articles_article_id",
                        column: x => x.article_id,
                        principalTable: "section_articles",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_article_progresses_article_id",
                table: "article_progresses",
                column: "article_id");

            migrationBuilder.CreateIndex(
                name: "ix_article_progresses_module_progress_id",
                table: "article_progresses",
                column: "module_progress_id");

            migrationBuilder.CreateIndex(
                name: "ix_challenge_progresses_challenge_id",
                table: "challenge_progresses",
                column: "challenge_id");

            migrationBuilder.CreateIndex(
                name: "ix_challenge_step_progresses_challenge_progress_id",
                table: "challenge_step_progresses",
                column: "challenge_progress_id");

            migrationBuilder.CreateIndex(
                name: "ix_challenge_step_progresses_challenge_step_id",
                table: "challenge_step_progresses",
                column: "challenge_step_id");

            migrationBuilder.CreateIndex(
                name: "ix_challenge_step_progresses_post_mood_id",
                table: "challenge_step_progresses",
                column: "post_mood_id");

            migrationBuilder.CreateIndex(
                name: "ix_challenge_steps_activity_id",
                table: "challenge_steps",
                column: "activity_id");

            migrationBuilder.CreateIndex(
                name: "ix_challenge_steps_challenge_id",
                table: "challenge_steps",
                column: "challenge_id");

            migrationBuilder.CreateIndex(
                name: "ix_journal_moods_mood_id",
                table: "journal_moods",
                column: "mood_id");

            migrationBuilder.CreateIndex(
                name: "ix_module_progresses_section_id",
                table: "module_progresses",
                column: "section_id");

            migrationBuilder.CreateIndex(
                name: "ix_module_sections_module_id",
                table: "module_sections",
                column: "module_id");

            migrationBuilder.CreateIndex(
                name: "ix_process_histories_activity_id",
                table: "process_histories",
                column: "activity_id");

            migrationBuilder.CreateIndex(
                name: "ix_process_histories_challenge_id",
                table: "process_histories",
                column: "challenge_id");

            migrationBuilder.CreateIndex(
                name: "ix_process_histories_post_mood_id",
                table: "process_histories",
                column: "post_mood_id");

            migrationBuilder.CreateIndex(
                name: "ix_section_articles_section_id",
                table: "section_articles",
                column: "section_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "article_progresses");

            migrationBuilder.DropTable(
                name: "challenge_step_progresses");

            migrationBuilder.DropTable(
                name: "idempotency_keys");

            migrationBuilder.DropTable(
                name: "journal_moods");

            migrationBuilder.DropTable(
                name: "outbox_messages");

            migrationBuilder.DropTable(
                name: "process_histories");

            migrationBuilder.DropTable(
                name: "module_progresses");

            migrationBuilder.DropTable(
                name: "section_articles");

            migrationBuilder.DropTable(
                name: "challenge_progresses");

            migrationBuilder.DropTable(
                name: "challenge_steps");

            migrationBuilder.DropTable(
                name: "moods");

            migrationBuilder.DropTable(
                name: "module_sections");

            migrationBuilder.DropTable(
                name: "activities");

            migrationBuilder.DropTable(
                name: "challenges");

            migrationBuilder.DropTable(
                name: "wellness_modules");
        }
    }
}
