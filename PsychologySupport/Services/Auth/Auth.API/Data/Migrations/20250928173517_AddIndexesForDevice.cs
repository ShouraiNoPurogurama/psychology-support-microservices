using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auth.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexesForDevice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_devices_user_id",
                table: "devices");

            migrationBuilder.CreateIndex(
                name: "ix_devices_user_device_type",
                table: "devices",
                columns: new[] { "user_id", "device_type" });

            migrationBuilder.CreateIndex(
                name: "uq_devices_client_user",
                table: "devices",
                columns: new[] { "client_device_id", "user_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_devices_user_device_type",
                table: "devices");

            migrationBuilder.DropIndex(
                name: "uq_devices_client_user",
                table: "devices");

            migrationBuilder.CreateIndex(
                name: "ix_devices_user_id",
                table: "devices",
                column: "user_id");
        }
    }
}
