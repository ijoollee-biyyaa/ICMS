using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Icms.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEfgbcIdSequence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("CREATE SEQUENCE \"EfgbcMemberIdSeq\" START 1 INCREMENT 1;");
            migrationBuilder.Sql("CREATE SEQUENCE \"ClearanceCertSeq\" START 1 INCREMENT 1;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP SEQUENCE IF EXISTS \"EfgbcMemberIdSeq\";");
            migrationBuilder.Sql("DROP SEQUENCE IF EXISTS \"ClearanceCertSeq\";");
        }
    }
}
