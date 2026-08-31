using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Icms.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMainTeamTypeUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_Teams_ChurchId_ParentTeamId_Name",
                table: "Teams",
                newName: "IX_Teams_SubName");

            migrationBuilder.RenameIndex(
                name: "IX_Teams_ChurchId_Name",
                table: "Teams",
                newName: "IX_Teams_MainName");

            migrationBuilder.CreateIndex(
                name: "IX_Teams_MainType",
                table: "Teams",
                columns: new[] { "ChurchId", "TeamType" },
                unique: true,
                filter: "\"ParentTeamId\" IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Teams_MainType",
                table: "Teams");

            migrationBuilder.RenameIndex(
                name: "IX_Teams_SubName",
                table: "Teams",
                newName: "IX_Teams_ChurchId_ParentTeamId_Name");

            migrationBuilder.RenameIndex(
                name: "IX_Teams_MainName",
                table: "Teams",
                newName: "IX_Teams_ChurchId_Name");
        }
    }
}
