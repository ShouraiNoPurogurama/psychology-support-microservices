using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Wellness.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RefactorIdempotencyKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "idempotency_key1",
                table: "idempotency_keys");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "expires_at",
                table: "idempotency_keys",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<string>(
                name: "created_by",
                table: "idempotency_keys",
                type: "text",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "key",
                table: "idempotency_keys",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "key",
                table: "idempotency_keys");

            migrationBuilder.AlterColumn<DateTime>(
                name: "expires_at",
                table: "idempotency_keys",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "created_by",
                table: "idempotency_keys",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "idempotency_key1",
                table: "idempotency_keys",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
