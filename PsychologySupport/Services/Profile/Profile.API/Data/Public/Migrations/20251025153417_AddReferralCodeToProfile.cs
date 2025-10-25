using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Profile.API.Data.Public.Migrations
{
    /// <inheritdoc />
    public partial class AddReferralCodeToProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "referral_code",
                schema: "public",
                table: "patient_profiles",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "referral_code",
                schema: "public",
                table: "patient_profiles");
        }
    }
}
