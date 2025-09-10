using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Billing.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class Update_Status : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "orders",
                type: "VARCHAR(20)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldDefaultValueSql: "'Pending'::character varying");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "invoices",
                type: "VARCHAR(20)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldDefaultValueSql: "'Issued'::character varying");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "orders",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValueSql: "'Pending'::character varying",
                oldClrType: typeof(string),
                oldType: "VARCHAR(20)");

            migrationBuilder.AlterColumn<string>(
                name: "status",
                table: "invoices",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValueSql: "'Issued'::character varying",
                oldClrType: typeof(string),
                oldType: "VARCHAR(20)");
        }
    }
}
