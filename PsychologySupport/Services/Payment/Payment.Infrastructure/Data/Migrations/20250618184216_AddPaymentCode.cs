using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payment.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentCode : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE SEQUENCE IF NOT EXISTS payment_code_seq START 300000001;
            ");

            migrationBuilder.Sql(@"
                DO $$
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1
                        FROM information_schema.columns
                        WHERE table_name = 'Payments' AND column_name = 'PaymentCode'
                    ) THEN
                        ALTER TABLE ""Payments"" ADD ""PaymentCode"" bigint NOT NULL DEFAULT nextval('payment_code_seq');
                    END IF;
                END
                $$;
            ");
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
}
