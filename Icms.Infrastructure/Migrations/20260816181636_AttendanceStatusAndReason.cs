using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Icms.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AttendanceStatusAndReason : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "TeamAttendances",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Reason",
                table: "TeamAttendances",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.Sql(
                "UPDATE \"TeamAttendances\" SET \"Status\" = 2 WHERE \"Attended\" = false;");

            migrationBuilder.Sql(
                "UPDATE \"TeamAttendances\" SET \"Status\" = 0 WHERE \"Attended\" = true;");

            migrationBuilder.DropColumn(
                name: "Attended",
                table: "TeamAttendances");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Attended",
                table: "TeamAttendances",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql(
                "UPDATE \"TeamAttendances\" SET \"Attended\" = true WHERE \"Status\" IN (0, 1);");

            migrationBuilder.Sql(
                "UPDATE \"TeamAttendances\" SET \"Attended\" = false WHERE \"Status\" = 2;");

            migrationBuilder.DropColumn(
                name: "Reason",
                table: "TeamAttendances");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "TeamAttendances");
        }
    }
}