using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    public partial class updateClientsTableChangeSuccessManagerColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SuccessManagerId",
                table: "CLIENT");

            migrationBuilder.AddColumn<int>(
                name: "SuccessManager",
                table: "CLIENT",
                type: "int",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SuccessManager",
                table: "CLIENT");

            migrationBuilder.AddColumn<string>(
                name: "SuccessManagerId",
                table: "CLIENT",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);
        }
    }
}
