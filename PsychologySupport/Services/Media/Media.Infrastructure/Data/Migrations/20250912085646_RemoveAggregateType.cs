using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Media.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAggregateType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "aggregate_type",
                table: "outbox_messages");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "aggregate_type",
                table: "outbox_messages",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
