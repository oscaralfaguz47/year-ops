using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    public partial class deleteConsultantClientsTableFinal : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
        name: "ConsultantClient"
    );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
