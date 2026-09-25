using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Icms.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMemberPersonalDetailsAndClearanceLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "BaptismDate",
                table: "Members",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BaptismPlace",
                table: "Members",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Members",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ClearanceId",
                table: "Members",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "ConversionDate",
                table: "Members",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HealthStatus",
                table: "Members",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "LocalAddress",
                table: "Members",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaritalStatus",
                table: "Members",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SpiritualGift",
                table: "Members",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Subcity",
                table: "Members",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BaptismDate",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "BaptismPlace",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "City",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "ClearanceId",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "ConversionDate",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "HealthStatus",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "LocalAddress",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "MaritalStatus",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "SpiritualGift",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "Subcity",
                table: "Members");
        }
    }
}
