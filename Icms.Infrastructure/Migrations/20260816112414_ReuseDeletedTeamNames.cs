using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Icms.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ReuseDeletedTeamNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "DROP INDEX \"IX_Teams_Name\";");

            migrationBuilder.Sql(
                "CREATE UNIQUE INDEX \"IX_Teams_Name\" ON \"Teams\" (\"ChurchId\", LOWER(\"Name\")) WHERE \"IsDeleted\" = false;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "DROP INDEX \"IX_Teams_Name\";");

            migrationBuilder.Sql(
                "CREATE UNIQUE INDEX \"IX_Teams_Name\" ON \"Teams\" (\"ChurchId\", LOWER(\"Name\"));");
        }
    }
}