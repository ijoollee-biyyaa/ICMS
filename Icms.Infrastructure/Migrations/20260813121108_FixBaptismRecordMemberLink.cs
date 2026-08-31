using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Icms.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixBaptismRecordMemberLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BaptismRecords_MemberId",
                table: "BaptismRecords");

            migrationBuilder.AlterColumn<long>(
                name: "MemberId",
                table: "BaptismRecords",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.CreateIndex(
                name: "IX_BaptismRecords_MemberId",
                table: "BaptismRecords",
                column: "MemberId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BaptismRecords_MemberId",
                table: "BaptismRecords");

            migrationBuilder.AlterColumn<long>(
                name: "MemberId",
                table: "BaptismRecords",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BaptismRecords_MemberId",
                table: "BaptismRecords",
                column: "MemberId",
                unique: true);
        }
    }
}
