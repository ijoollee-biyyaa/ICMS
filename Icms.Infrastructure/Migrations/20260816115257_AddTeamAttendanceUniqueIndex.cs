using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Icms.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTeamAttendanceUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_TeamAttendances_TeamId_MemberId_AttendanceDate",
                table: "TeamAttendances",
                columns: new[] { "TeamId", "MemberId", "AttendanceDate" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TeamAttendances_TeamId_MemberId_AttendanceDate",
                table: "TeamAttendances");
        }
    }
}
