using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Icms.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefineBaptismRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BaptismRecords_MemberId",
                table: "BaptismRecords");

            migrationBuilder.RenameColumn(
                name: "BirthPlace",
                table: "BaptismRecords",
                newName: "PlaceOfBirth");

            migrationBuilder.AddColumn<string>(
                name: "ChildName",
                table: "BaptismRecords",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateOnly>(
                name: "DateOfBirth",
                table: "BaptismRecords",
                type: "date",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BaptismRecords_MemberId",
                table: "BaptismRecords",
                column: "MemberId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BaptismRecords_MemberId",
                table: "BaptismRecords");

            migrationBuilder.DropColumn(
                name: "ChildName",
                table: "BaptismRecords");

            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "BaptismRecords");

            migrationBuilder.RenameColumn(
                name: "PlaceOfBirth",
                table: "BaptismRecords",
                newName: "BirthPlace");

            migrationBuilder.CreateIndex(
                name: "IX_BaptismRecords_MemberId",
                table: "BaptismRecords",
                column: "MemberId");
        }
    }
}
