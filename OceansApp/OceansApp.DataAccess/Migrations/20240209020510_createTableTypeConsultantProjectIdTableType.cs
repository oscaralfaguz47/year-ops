using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    public partial class createTableTypeConsultantProjectIdTableType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
        CREATE TYPE ConsultantProjectIdTableType AS TABLE (ConsultantProjectId INT);
    ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TYPE IF EXISTS ConsultantProjectIdTableType;");
        }
    }
}
