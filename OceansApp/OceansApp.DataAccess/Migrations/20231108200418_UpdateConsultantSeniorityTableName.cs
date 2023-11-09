using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    public partial class UpdateConsultantSeniorityTableName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_CONSULTANT_SENIORITS",
                table: "CONSULTANT_SENIORITS");

            migrationBuilder.RenameTable(
                name: "CONSULTANT_SENIORITS",
                newName: "CONSULTANT_SENIORITIS");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CONSULTANT_SENIORITIS",
                table: "CONSULTANT_SENIORITIS",
                column: "ConsultantSeniorityId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_CONSULTANT_SENIORITIS",
                table: "CONSULTANT_SENIORITIS");

            migrationBuilder.RenameTable(
                name: "CONSULTANT_SENIORITIS",
                newName: "CONSULTANT_SENIORITS");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CONSULTANT_SENIORITS",
                table: "CONSULTANT_SENIORITS",
                column: "ConsultantSeniorityId");
        }
    }
}
