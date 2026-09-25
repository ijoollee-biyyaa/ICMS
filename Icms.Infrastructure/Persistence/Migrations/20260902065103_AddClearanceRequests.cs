using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Icms.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddClearanceRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClearanceRequests",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChurchId = table.Column<long>(type: "bigint", nullable: false),
                    MemberId = table.Column<long>(type: "bigint", nullable: true),
                    Direction = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CertificateCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    DestinationChurchId = table.Column<long>(type: "bigint", nullable: true),
                    DestinationChurchName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DestinationDistrictName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ExternalChurchName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ExternalDistrictName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PreviousEfgbcId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    IncomingFirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IncomingFatherName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IncomingGrandfatherName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FileUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    RejectionReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    InitiatedByUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    InitiatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ResolvedByUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    ResolvedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClearanceRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClearanceRequests_Churches_ChurchId",
                        column: x => x.ChurchId,
                        principalTable: "Churches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClearanceRequests_Churches_DestinationChurchId",
                        column: x => x.DestinationChurchId,
                        principalTable: "Churches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClearanceRequests_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClearanceRequests_CertificateCode",
                table: "ClearanceRequests",
                column: "CertificateCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClearanceRequests_ChurchId",
                table: "ClearanceRequests",
                column: "ChurchId");

            migrationBuilder.CreateIndex(
                name: "IX_ClearanceRequests_DestinationChurchId",
                table: "ClearanceRequests",
                column: "DestinationChurchId");

            migrationBuilder.CreateIndex(
                name: "IX_ClearanceRequests_MemberId",
                table: "ClearanceRequests",
                column: "MemberId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClearanceRequests");
        }
    }
}
