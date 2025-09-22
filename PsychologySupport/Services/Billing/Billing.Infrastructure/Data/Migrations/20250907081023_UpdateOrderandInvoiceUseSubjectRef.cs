using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Billing.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateOrderandInvoiceUseSubjectRef : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Đổi tên cột alias_id -> subject_ref trong orders
            migrationBuilder.RenameColumn(
                name: "alias_id",
                table: "orders",
                newName: "subject_ref");

            // Đổi tên cột alias_id -> subject_ref trong invoices
            migrationBuilder.RenameColumn(
                name: "alias_id",
                table: "invoices",
                newName: "subject_ref");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Rollback: đổi lại subject_ref -> alias_id
            migrationBuilder.RenameColumn(
                name: "subject_ref",
                table: "orders",
                newName: "alias_id");

            migrationBuilder.RenameColumn(
                name: "subject_ref",
                table: "invoices",
                newName: "alias_id");
        }
    }
}
