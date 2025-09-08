using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitalGoods.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEmotionTagSoT : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "emotion_tags",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "text", nullable: false),
                    display_name = table.Column<string>(type: "text", nullable: false),
                    media_id = table.Column<Guid>(type: "uuid", nullable: true),
                    topic = table.Column<string>(type: "text", nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    last_modified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_modified_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_emotion_tags", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "outbox_messages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    occured_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    processed_on = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created_by = table.Column<string>(type: "text", nullable: true),
                    last_modified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    last_modified_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_outbox_messages", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "digital_good_emotion_tag",
                columns: table => new
                {
                    digital_goods_id = table.Column<Guid>(type: "uuid", nullable: false),
                    emotion_tags_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_digital_good_emotion_tag", x => new { x.digital_goods_id, x.emotion_tags_id });
                    table.ForeignKey(
                        name: "fk_digital_good_emotion_tag_digital_goods_digital_goods_id",
                        column: x => x.digital_goods_id,
                        principalTable: "digital_goods",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_digital_good_emotion_tag_emotion_tags_emotion_tags_id",
                        column: x => x.emotion_tags_id,
                        principalTable: "emotion_tags",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_digital_good_emotion_tag_emotion_tags_id",
                table: "digital_good_emotion_tag",
                column: "emotion_tags_id");

            migrationBuilder.CreateIndex(
                name: "ix_emotion_tags_code",
                table: "emotion_tags",
                column: "code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "digital_good_emotion_tag");

            migrationBuilder.DropTable(
                name: "outbox_messages");

            migrationBuilder.DropTable(
                name: "emotion_tags");
        }
    }
}
