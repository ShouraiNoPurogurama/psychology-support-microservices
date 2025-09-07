using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Billing.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateIdempotencyKeyToGuid : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "idempotency_key",
                table: "idempotency_keys",
                newName: "key");

            migrationBuilder.RenameIndex(
                name: "idempotency_keys_idempotency_key_key",
                table: "idempotency_keys",
                newName: "idempotency_keys_key_key");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "key",
                table: "idempotency_keys",
                newName: "idempotency_key");

            migrationBuilder.RenameIndex(
                name: "idempotency_keys_key_key",
                table: "idempotency_keys",
                newName: "idempotency_keys_idempotency_key_key");
        }


    }
}
