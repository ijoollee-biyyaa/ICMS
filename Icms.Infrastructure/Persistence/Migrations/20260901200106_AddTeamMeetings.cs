using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Icms.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTeamMeetings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TeamAttendances_TeamId_MemberId_AttendanceDate",
                table: "TeamAttendances");

            migrationBuilder.AddColumn<long>(
                name: "MeetingId",
                table: "TeamAttendances",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "TeamMeetings",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TeamId = table.Column<long>(type: "bigint", nullable: false),
                    MeetingDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedById = table.Column<long>(type: "bigint", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeamMeetings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TeamMeetings_Members_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TeamMeetings_Teams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "Teams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // Backfill: convert every existing attendance date into a meeting record, then attach
            // attendance rows to it. AttendanceDate stays as a denormalized copy of the meeting's date.
            migrationBuilder.Sql("""
                INSERT INTO "TeamMeetings" ("TeamId", "MeetingDate", "Title", "Notes", "CreatedAt")
                SELECT DISTINCT a."TeamId", a."AttendanceDate", NULL, NULL, now()
                FROM "TeamAttendances" a
                WHERE a."AttendanceDate" IS NOT NULL;

                UPDATE "TeamAttendances" a
                SET "MeetingId" = m."Id"
                FROM "TeamMeetings" m
                WHERE a."TeamId" = m."TeamId" AND a."AttendanceDate" = m."MeetingDate";
                """);

            migrationBuilder.CreateIndex(
                name: "IX_TeamAttendances_MeetingId_MemberId",
                table: "TeamAttendances",
                columns: new[] { "MeetingId", "MemberId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TeamMeetings_CreatedById",
                table: "TeamMeetings",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_TeamMeetings_TeamId_MeetingDate",
                table: "TeamMeetings",
                columns: new[] { "TeamId", "MeetingDate" });

            migrationBuilder.AddForeignKey(
                name: "FK_TeamAttendances_TeamMeetings_MeetingId",
                table: "TeamAttendances",
                column: "MeetingId",
                principalTable: "TeamMeetings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TeamAttendances_TeamMeetings_MeetingId",
                table: "TeamAttendances");

            migrationBuilder.DropTable(
                name: "TeamMeetings");

            migrationBuilder.DropIndex(
                name: "IX_TeamAttendances_MeetingId_MemberId",
                table: "TeamAttendances");

            migrationBuilder.DropColumn(
                name: "MeetingId",
                table: "TeamAttendances");

            migrationBuilder.CreateIndex(
                name: "IX_TeamAttendances_TeamId_MemberId_AttendanceDate",
                table: "TeamAttendances",
                columns: new[] { "TeamId", "MemberId", "AttendanceDate" },
                unique: true);
        }
    }
}
