using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Icms.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountLockStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAccountLocked",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "LockReason",
                table: "AspNetUsers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LockedAtUtc",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAccountLocked",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LockReason",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LockedAtUtc",
                table: "AspNetUsers");
        }
    }
}
