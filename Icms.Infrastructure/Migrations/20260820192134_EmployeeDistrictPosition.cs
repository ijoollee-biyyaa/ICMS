using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Icms.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EmployeeDistrictPosition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChurchId",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<long>(
                name: "DistrictId",
                table: "Employees",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_DistrictId",
                table: "Employees",
                column: "DistrictId");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Districts_DistrictId",
                table: "Employees",
                column: "DistrictId",
                principalTable: "Districts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Districts_DistrictId",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Employees_DistrictId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "DistrictId",
                table: "Employees");

            migrationBuilder.AddColumn<long>(
                name: "ChurchId",
                table: "AspNetUsers",
                type: "bigint",
                nullable: true);
        }
    }
}
