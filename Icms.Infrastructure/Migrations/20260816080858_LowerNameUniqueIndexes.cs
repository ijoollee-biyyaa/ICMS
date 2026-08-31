using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Icms.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class LowerNameUniqueIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Teams_MainName",
                table: "Teams");

            migrationBuilder.DropIndex(
                name: "IX_Teams_SubName",
                table: "Teams");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:PostgresExtension:citext", ",,");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Teams",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "citext",
                oldMaxLength: 150);

            migrationBuilder.CreateIndex(
                name: "IX_Teams_ChurchId",
                table: "Teams",
                column: "ChurchId");

            migrationBuilder.Sql(
                "CREATE UNIQUE INDEX \"IX_Teams_MainName\" ON \"Teams\" (\"ChurchId\", LOWER(\"Name\")) WHERE \"ParentTeamId\" IS NULL;");

            migrationBuilder.Sql(
                "CREATE UNIQUE INDEX \"IX_Teams_SubName\" ON \"Teams\" (\"ChurchId\", \"ParentTeamId\", LOWER(\"Name\")) WHERE \"ParentTeamId\" IS NOT NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "DROP INDEX \"IX_Teams_MainName\";");

            migrationBuilder.Sql(
                "DROP INDEX \"IX_Teams_SubName\";");

            migrationBuilder.DropIndex(
                name: "IX_Teams_ChurchId",
                table: "Teams");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:citext", ",,");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Teams",
                type: "citext",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(150)",
                oldMaxLength: 150);

            migrationBuilder.CreateIndex(
                name: "IX_Teams_MainName",
                table: "Teams",
                columns: new[] { "ChurchId", "Name" },
                unique: true,
                filter: "\"ParentTeamId\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Teams_SubName",
                table: "Teams",
                columns: new[] { "ChurchId", "ParentTeamId", "Name" },
                unique: true,
                filter: "\"ParentTeamId\" IS NOT NULL");
        }
    }
}
