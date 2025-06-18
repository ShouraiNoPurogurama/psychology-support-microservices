using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payment.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentCode : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("CREATE SEQUENCE IF NOT EXISTS payment_code_seq START 300000001;");

            migrationBuilder.AddColumn<long>(
                name: "PaymentCode",
                table: "Payments",
                type: "bigint",
                nullable: false,
                defaultValueSql: "nextval('payment_code_seq')"
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentCode",
                table: "Payments");

            migrationBuilder.Sql("DROP SEQUENCE IF EXISTS payment_code_seq;");
        }
    }
}
