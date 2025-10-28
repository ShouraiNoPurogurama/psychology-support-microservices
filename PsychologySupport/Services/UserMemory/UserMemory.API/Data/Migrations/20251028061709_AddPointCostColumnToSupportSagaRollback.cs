using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserMemory.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPointCostColumnToSupportSagaRollback : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "points_cost",
                table: "rewards",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "session_id",
                table: "rewards",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "points_cost",
                table: "rewards");

            migrationBuilder.DropColumn(
                name: "session_id",
                table: "rewards");
        }
    }
}
