using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Post.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AdjustDateTimeFormat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "request_fingerprint",
                schema: "public",
                table: "idempotency_keys");

            migrationBuilder.AlterColumn<Guid>(
                name: "author_alias_version_id",
                schema: "public",
                table: "posts",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "expires_at",
                schema: "public",
                table: "idempotency_keys",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "request_hash",
                schema: "public",
                table: "idempotency_keys",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "response_payload",
                schema: "public",
                table: "idempotency_keys",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "expires_at",
                schema: "public",
                table: "idempotency_keys");

            migrationBuilder.DropColumn(
                name: "request_hash",
                schema: "public",
                table: "idempotency_keys");

            migrationBuilder.DropColumn(
                name: "response_payload",
                schema: "public",
                table: "idempotency_keys");

            migrationBuilder.AlterColumn<Guid>(
                name: "author_alias_version_id",
                schema: "public",
                table: "posts",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "request_fingerprint",
                schema: "public",
                table: "idempotency_keys",
                type: "bytea",
                nullable: false,
                defaultValue: new byte[0]);
        }
    }
}
