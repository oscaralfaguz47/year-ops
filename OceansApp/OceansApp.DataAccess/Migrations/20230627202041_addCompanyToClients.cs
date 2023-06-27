using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    public partial class addCompanyToClients : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_CLIENT",
                table: "CLIENT");

            migrationBuilder.RenameColumn(
                name: "IdClient",
                table: "CLIENT",
                newName: "ClientCode");

            migrationBuilder.AddColumn<int>(
                name: "ClientId",
                table: "CLIENT",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<string>(
                name: "CompanyId",
                table: "CLIENT",
                type: "nvarchar(8)",
                maxLength: 8,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CLIENT",
                table: "CLIENT",
                column: "ClientId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_CLIENT",
                table: "CLIENT");

            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "CLIENT");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "CLIENT");

            migrationBuilder.RenameColumn(
                name: "ClientCode",
                table: "CLIENT",
                newName: "IdClient");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CLIENT",
                table: "CLIENT",
                column: "IdClient");
        }
    }
}
