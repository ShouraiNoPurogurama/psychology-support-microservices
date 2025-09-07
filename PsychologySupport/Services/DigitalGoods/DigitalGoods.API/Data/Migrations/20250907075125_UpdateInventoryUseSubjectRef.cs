using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DigitalGoods.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateInventoryUseSubjectRef : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Rename column user_id -> subject_ref
            migrationBuilder.RenameColumn(
                name: "user_id",
                table: "inventories",
                newName: "subject_ref");

            // Rename unique index để khớp với cột mới
            migrationBuilder.RenameIndex(
                name: "uq_inventory_user_good_active",
                table: "inventories",
                newName: "ix_inventories_subject_ref_digital_good_id_status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Rollback ngược lại nếu cần
            migrationBuilder.RenameIndex(
                name: "ix_inventories_subject_ref_digital_good_id_status",
                table: "inventories",
                newName: "uq_inventory_user_good_active");

            migrationBuilder.RenameColumn(
                name: "subject_ref",
                table: "inventories",
                newName: "user_id");
        }
    }
}
