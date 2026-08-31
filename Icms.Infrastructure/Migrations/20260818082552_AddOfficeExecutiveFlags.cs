using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Icms.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOfficeExecutiveFlags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsVicePresident",
                table: "Employees",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_IsDistrictPresident",
                table: "Employees",
                column: "IsDistrictPresident",
                unique: true,
                filter: "\"IsDistrictPresident\" = true AND \"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_IsVicePresident",
                table: "Employees",
                column: "IsVicePresident",
                unique: true,
                filter: "\"IsVicePresident\" = true AND \"IsDeleted\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Employees_IsDistrictPresident",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Employees_IsVicePresident",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "IsVicePresident",
                table: "Employees");
        }
    }
}
