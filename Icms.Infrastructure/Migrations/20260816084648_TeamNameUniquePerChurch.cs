using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Icms.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TeamNameUniquePerChurch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "DROP INDEX \"IX_Teams_MainName\";");

            migrationBuilder.Sql(
                "DROP INDEX \"IX_Teams_SubName\";");

            migrationBuilder.Sql(
                "CREATE UNIQUE INDEX \"IX_Teams_Name\" ON \"Teams\" (\"ChurchId\", LOWER(\"Name\"));");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "DROP INDEX \"IX_Teams_Name\";");

            migrationBuilder.Sql(
                "CREATE UNIQUE INDEX \"IX_Teams_MainName\" ON \"Teams\" (\"ChurchId\", LOWER(\"Name\")) WHERE \"ParentTeamId\" IS NULL;");

            migrationBuilder.Sql(
                "CREATE UNIQUE INDEX \"IX_Teams_SubName\" ON \"Teams\" (\"ChurchId\", \"ParentTeamId\", LOWER(\"Name\")) WHERE \"ParentTeamId\" IS NOT NULL;");
        }
    }
}