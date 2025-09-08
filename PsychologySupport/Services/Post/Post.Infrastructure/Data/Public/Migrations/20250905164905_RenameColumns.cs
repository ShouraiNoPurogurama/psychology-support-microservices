using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Post.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "idempotency_key",
                table: "idempotency_keys",
                newName: "key");

            migrationBuilder.RenameIndex(
                name: "idempotency_keys_idempotency_key_key",
                table: "idempotency_keys",
                newName: "ix_idempotency_keys_key");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "key",
                table: "idempotency_keys",
                newName: "idempotency_key");

            migrationBuilder.RenameIndex(
                name: "ix_idempotency_keys_key",
                table: "idempotency_keys",
                newName: "idempotency_keys_idempotency_key_key");
        }
    }
}
