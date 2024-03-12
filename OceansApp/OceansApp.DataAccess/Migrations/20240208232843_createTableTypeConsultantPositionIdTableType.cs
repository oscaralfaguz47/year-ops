using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    public partial class createTableTypeConsultantPositionIdTableType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
        CREATE TYPE ConsultantPositionIdTableType AS TABLE (ConsultantPositionId INT);
    ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TYPE IF EXISTS ConsultantPositionIdTableType;");
        }
    }
}
