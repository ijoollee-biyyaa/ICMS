using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Icms.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserChurchId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ChurchId",
                table: "AspNetUsers",
                type: "bigint",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChurchId",
                table: "AspNetUsers");
        }
    }
}
