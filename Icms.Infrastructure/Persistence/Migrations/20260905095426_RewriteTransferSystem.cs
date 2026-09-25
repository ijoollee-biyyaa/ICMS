using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Icms.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RewriteTransferSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transfers_Churches_FromChurchId",
                table: "Transfers");

            migrationBuilder.DropForeignKey(
                name: "FK_Transfers_Churches_ToChurchId",
                table: "Transfers");

            migrationBuilder.DropTable(
                name: "ClearanceCertificates");

            migrationBuilder.DropTable(
                name: "ClearanceRequests");

            migrationBuilder.DropIndex(
                name: "IX_Transfers_FromChurchId_Status",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "FromChurchId",
                table: "Transfers");

            migrationBuilder.RenameColumn(
                name: "ToChurchId",
                table: "Transfers",
                newName: "SourceChurchId");

            migrationBuilder.RenameColumn(
                name: "InitiatedBy",
                table: "Transfers",
                newName: "Direction");

            migrationBuilder.RenameColumn(
                name: "DestinationName",
                table: "Transfers",
                newName: "SourceDistrictOrDenomination");

            migrationBuilder.RenameColumn(
                name: "ClosedAt",
                table: "Transfers",
                newName: "CompletedAt");

            migrationBuilder.RenameIndex(
                name: "IX_Transfers_ToChurchId",
                table: "Transfers",
                newName: "IX_Transfers_SourceChurchId");

            migrationBuilder.AlterColumn<string>(
                name: "VoidedByUserId",
                table: "Transfers",
                type: "character varying(450)",
                maxLength: 450,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "VoidReason",
                table: "Transfers",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "MemberId",
                table: "Transfers",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<string>(
                name: "InitiatedByUserId",
                table: "Transfers",
                type: "character varying(450)",
                maxLength: 450,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClearanceCode",
                table: "Transfers",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ClearanceDocumentUrl",
                table: "Transfers",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompletedByUserId",
                table: "Transfers",
                type: "character varying(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DestinationChurchId",
                table: "Transfers",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DestinationChurchName",
                table: "Transfers",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DestinationDistrictOrDenomination",
                table: "Transfers",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IncomingFatherName",
                table: "Transfers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IncomingFirstName",
                table: "Transfers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IncomingGrandfatherName",
                table: "Transfers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "InitiatedAt",
                table: "Transfers",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "PreviousEfgbcId",
                table: "Transfers",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RecommendationNotes",
                table: "Transfers",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceChurchName",
                table: "Transfers",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TransferSnapshots",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TransferId = table.Column<long>(type: "bigint", nullable: false),
                    MemberId = table.Column<long>(type: "bigint", nullable: false),
                    TeamsJson = table.Column<string>(type: "text", nullable: false),
                    DepartmentsJson = table.Column<string>(type: "text", nullable: false),
                    PaymentSummaryJson = table.Column<string>(type: "text", nullable: false),
                    AttendanceSummaryJson = table.Column<string>(type: "text", nullable: false),
                    SnapshotAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransferSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TransferSnapshots_Transfers_TransferId",
                        column: x => x.TransferId,
                        principalTable: "Transfers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_ClearanceCode",
                table: "Transfers",
                column: "ClearanceCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_DestinationChurchId",
                table: "Transfers",
                column: "DestinationChurchId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferSnapshots_MemberId",
                table: "TransferSnapshots",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_TransferSnapshots_TransferId",
                table: "TransferSnapshots",
                column: "TransferId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Transfers_Churches_DestinationChurchId",
                table: "Transfers",
                column: "DestinationChurchId",
                principalTable: "Churches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Transfers_Churches_SourceChurchId",
                table: "Transfers",
                column: "SourceChurchId",
                principalTable: "Churches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transfers_Churches_DestinationChurchId",
                table: "Transfers");

            migrationBuilder.DropForeignKey(
                name: "FK_Transfers_Churches_SourceChurchId",
                table: "Transfers");

            migrationBuilder.DropTable(
                name: "TransferSnapshots");

            migrationBuilder.DropIndex(
                name: "IX_Transfers_ClearanceCode",
                table: "Transfers");

            migrationBuilder.DropIndex(
                name: "IX_Transfers_DestinationChurchId",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "ClearanceCode",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "ClearanceDocumentUrl",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "CompletedByUserId",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "DestinationChurchId",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "DestinationChurchName",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "DestinationDistrictOrDenomination",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "IncomingFatherName",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "IncomingFirstName",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "IncomingGrandfatherName",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "InitiatedAt",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "PreviousEfgbcId",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "RecommendationNotes",
                table: "Transfers");

            migrationBuilder.DropColumn(
                name: "SourceChurchName",
                table: "Transfers");

            migrationBuilder.RenameColumn(
                name: "SourceDistrictOrDenomination",
                table: "Transfers",
                newName: "DestinationName");

            migrationBuilder.RenameColumn(
                name: "SourceChurchId",
                table: "Transfers",
                newName: "ToChurchId");

            migrationBuilder.RenameColumn(
                name: "Direction",
                table: "Transfers",
                newName: "InitiatedBy");

            migrationBuilder.RenameColumn(
                name: "CompletedAt",
                table: "Transfers",
                newName: "ClosedAt");

            migrationBuilder.RenameIndex(
                name: "IX_Transfers_SourceChurchId",
                table: "Transfers",
                newName: "IX_Transfers_ToChurchId");

            migrationBuilder.AlterColumn<string>(
                name: "VoidedByUserId",
                table: "Transfers",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(450)",
                oldMaxLength: 450,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "VoidReason",
                table: "Transfers",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "MemberId",
                table: "Transfers",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "InitiatedByUserId",
                table: "Transfers",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(450)",
                oldMaxLength: 450,
                oldNullable: true);

            migrationBuilder.AddColumn<long>(
                name: "FromChurchId",
                table: "Transfers",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "ClearanceCertificates",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FromChurchId = table.Column<long>(type: "bigint", nullable: false),
                    MemberId = table.Column<long>(type: "bigint", nullable: false),
                    ToChurchId = table.Column<long>(type: "bigint", nullable: true),
                    TransferId = table.Column<long>(type: "bigint", nullable: false),
                    CertificateCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    IssuedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IssuedByUserId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClearanceCertificates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClearanceCertificates_Churches_FromChurchId",
                        column: x => x.FromChurchId,
                        principalTable: "Churches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClearanceCertificates_Churches_ToChurchId",
                        column: x => x.ToChurchId,
                        principalTable: "Churches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClearanceCertificates_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClearanceCertificates_Transfers_TransferId",
                        column: x => x.TransferId,
                        principalTable: "Transfers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClearanceRequests",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ChurchId = table.Column<long>(type: "bigint", nullable: false),
                    DestinationChurchId = table.Column<long>(type: "bigint", nullable: true),
                    MemberId = table.Column<long>(type: "bigint", nullable: true),
                    CertificateCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DestinationChurchName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DestinationDistrictName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Direction = table.Column<int>(type: "integer", nullable: false),
                    ExternalChurchName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ExternalDistrictName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    FileUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IncomingFatherName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IncomingFirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IncomingGrandfatherName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    InitiatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    InitiatedByUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Notes = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    PreviousEfgbcId = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    RejectionReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ResolvedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ResolvedByUserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
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
                name: "IX_Transfers_FromChurchId_Status",
                table: "Transfers",
                columns: new[] { "FromChurchId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ClearanceCertificates_CertificateCode",
                table: "ClearanceCertificates",
                column: "CertificateCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClearanceCertificates_FromChurchId",
                table: "ClearanceCertificates",
                column: "FromChurchId");

            migrationBuilder.CreateIndex(
                name: "IX_ClearanceCertificates_MemberId",
                table: "ClearanceCertificates",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_ClearanceCertificates_ToChurchId",
                table: "ClearanceCertificates",
                column: "ToChurchId");

            migrationBuilder.CreateIndex(
                name: "IX_ClearanceCertificates_TransferId",
                table: "ClearanceCertificates",
                column: "TransferId",
                unique: true);

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

            migrationBuilder.AddForeignKey(
                name: "FK_Transfers_Churches_FromChurchId",
                table: "Transfers",
                column: "FromChurchId",
                principalTable: "Churches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Transfers_Churches_ToChurchId",
                table: "Transfers",
                column: "ToChurchId",
                principalTable: "Churches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
