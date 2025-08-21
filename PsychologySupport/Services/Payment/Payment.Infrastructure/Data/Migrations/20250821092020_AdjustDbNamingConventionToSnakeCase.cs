using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Payment.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AdjustDbNamingConventionToSnakeCase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentDetails_Payments_PaymentId",
                table: "PaymentDetails");

            migrationBuilder.DropForeignKey(
                name: "FK_Payments_PaymentMethods_PaymentMethodId",
                table: "Payments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Payments",
                table: "Payments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PaymentMethods",
                table: "PaymentMethods");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PaymentDetails",
                table: "PaymentDetails");

            migrationBuilder.RenameTable(
                name: "Payments",
                newName: "payments");

            migrationBuilder.RenameTable(
                name: "PaymentMethods",
                newName: "payment_methods");

            migrationBuilder.RenameTable(
                name: "PaymentDetails",
                newName: "payment_details");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "payments",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "payments",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "TotalAmount",
                table: "payments",
                newName: "total_amount");

            migrationBuilder.RenameColumn(
                name: "SubscriptionId",
                table: "payments",
                newName: "subscription_id");

            migrationBuilder.RenameColumn(
                name: "PaymentUrl",
                table: "payments",
                newName: "payment_url");

            migrationBuilder.RenameColumn(
                name: "PaymentType",
                table: "payments",
                newName: "payment_type");

            migrationBuilder.RenameColumn(
                name: "PaymentMethodId",
                table: "payments",
                newName: "payment_method_id");

            migrationBuilder.RenameColumn(
                name: "PaymentCode",
                table: "payments",
                newName: "payment_code");

            migrationBuilder.RenameColumn(
                name: "PatientProfileId",
                table: "payments",
                newName: "patient_profile_id");

            migrationBuilder.RenameColumn(
                name: "LastModifiedBy",
                table: "payments",
                newName: "last_modified_by");

            migrationBuilder.RenameColumn(
                name: "LastModified",
                table: "payments",
                newName: "last_modified");

            migrationBuilder.RenameColumn(
                name: "CreatedBy",
                table: "payments",
                newName: "created_by");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "payments",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "BookingId",
                table: "payments",
                newName: "booking_id");

            migrationBuilder.RenameIndex(
                name: "IX_Payments_PaymentMethodId",
                table: "payments",
                newName: "ix_payments_payment_method_id");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "payment_methods",
                newName: "name");

            migrationBuilder.RenameColumn(
                name: "Details",
                table: "payment_methods",
                newName: "details");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "payment_methods",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "payment_details",
                newName: "status");

            migrationBuilder.RenameColumn(
                name: "Amount",
                table: "payment_details",
                newName: "amount");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "payment_details",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "PaymentId",
                table: "payment_details",
                newName: "payment_id");

            migrationBuilder.RenameColumn(
                name: "ExternalTransactionCode",
                table: "payment_details",
                newName: "external_transaction_code");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "payment_details",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "IX_PaymentDetails_PaymentId",
                table: "payment_details",
                newName: "ix_payment_details_payment_id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_payments",
                table: "payments",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_payment_methods",
                table: "payment_methods",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "pk_payment_details",
                table: "payment_details",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_payment_details_payments_payment_id",
                table: "payment_details",
                column: "payment_id",
                principalTable: "payments",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_payments_payment_methods_payment_method_id",
                table: "payments",
                column: "payment_method_id",
                principalTable: "payment_methods",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_payment_details_payments_payment_id",
                table: "payment_details");

            migrationBuilder.DropForeignKey(
                name: "fk_payments_payment_methods_payment_method_id",
                table: "payments");

            migrationBuilder.DropPrimaryKey(
                name: "pk_payments",
                table: "payments");

            migrationBuilder.DropPrimaryKey(
                name: "pk_payment_methods",
                table: "payment_methods");

            migrationBuilder.DropPrimaryKey(
                name: "pk_payment_details",
                table: "payment_details");

            migrationBuilder.RenameTable(
                name: "payments",
                newName: "Payments");

            migrationBuilder.RenameTable(
                name: "payment_methods",
                newName: "PaymentMethods");

            migrationBuilder.RenameTable(
                name: "payment_details",
                newName: "PaymentDetails");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "Payments",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "Payments",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "total_amount",
                table: "Payments",
                newName: "TotalAmount");

            migrationBuilder.RenameColumn(
                name: "subscription_id",
                table: "Payments",
                newName: "SubscriptionId");

            migrationBuilder.RenameColumn(
                name: "payment_url",
                table: "Payments",
                newName: "PaymentUrl");

            migrationBuilder.RenameColumn(
                name: "payment_type",
                table: "Payments",
                newName: "PaymentType");

            migrationBuilder.RenameColumn(
                name: "payment_method_id",
                table: "Payments",
                newName: "PaymentMethodId");

            migrationBuilder.RenameColumn(
                name: "payment_code",
                table: "Payments",
                newName: "PaymentCode");

            migrationBuilder.RenameColumn(
                name: "patient_profile_id",
                table: "Payments",
                newName: "PatientProfileId");

            migrationBuilder.RenameColumn(
                name: "last_modified_by",
                table: "Payments",
                newName: "LastModifiedBy");

            migrationBuilder.RenameColumn(
                name: "last_modified",
                table: "Payments",
                newName: "LastModified");

            migrationBuilder.RenameColumn(
                name: "created_by",
                table: "Payments",
                newName: "CreatedBy");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "Payments",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "booking_id",
                table: "Payments",
                newName: "BookingId");

            migrationBuilder.RenameIndex(
                name: "ix_payments_payment_method_id",
                table: "Payments",
                newName: "IX_Payments_PaymentMethodId");

            migrationBuilder.RenameColumn(
                name: "name",
                table: "PaymentMethods",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "details",
                table: "PaymentMethods",
                newName: "Details");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "PaymentMethods",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "status",
                table: "PaymentDetails",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "amount",
                table: "PaymentDetails",
                newName: "Amount");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "PaymentDetails",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "payment_id",
                table: "PaymentDetails",
                newName: "PaymentId");

            migrationBuilder.RenameColumn(
                name: "external_transaction_code",
                table: "PaymentDetails",
                newName: "ExternalTransactionCode");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "PaymentDetails",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "ix_payment_details_payment_id",
                table: "PaymentDetails",
                newName: "IX_PaymentDetails_PaymentId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Payments",
                table: "Payments",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PaymentMethods",
                table: "PaymentMethods",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PaymentDetails",
                table: "PaymentDetails",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentDetails_Payments_PaymentId",
                table: "PaymentDetails",
                column: "PaymentId",
                principalTable: "Payments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_PaymentMethods_PaymentMethodId",
                table: "Payments",
                column: "PaymentMethodId",
                principalTable: "PaymentMethods",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
