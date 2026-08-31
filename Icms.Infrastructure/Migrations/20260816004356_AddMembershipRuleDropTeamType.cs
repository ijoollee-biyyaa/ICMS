using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Icms.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMembershipRuleDropTeamType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Teams_MainType",
                table: "Teams");

            migrationBuilder.RenameColumn(
                name: "TeamType",
                table: "Teams",
                newName: "MembershipRule");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MembershipRule",
                table: "Teams",
                newName: "TeamType");

            migrationBuilder.CreateIndex(
                name: "IX_Teams_MainType",
                table: "Teams",
                columns: new[] { "ChurchId", "TeamType" },
                unique: true,
                filter: "\"ParentTeamId\" IS NULL");
        }
    }
}
