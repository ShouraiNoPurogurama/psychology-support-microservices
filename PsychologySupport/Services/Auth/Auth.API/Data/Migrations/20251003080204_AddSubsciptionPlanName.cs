using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auth.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSubsciptionPlanName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "subscription_plan_name",
                table: "AspNetUsers",
                type: "text",
                nullable: false, 
                defaultValue: "Free Plan"); 
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "subscription_plan_name",
                table: "AspNetUsers");
        }
    }

}
