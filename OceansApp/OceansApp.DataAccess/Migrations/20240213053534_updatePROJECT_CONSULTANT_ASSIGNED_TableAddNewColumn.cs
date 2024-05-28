using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    public partial class updatePROJECT_CONSULTANT_ASSIGNED_TableAddNewColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsMonthlySalaryCalculatedPerHour",
                table: "PROJECTS_CONSULTANTS_ASSIGNED",
                type: "bit",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsMonthlySalaryCalculatedPerHour",
                table: "PROJECTS_CONSULTANTS_ASSIGNED");
        }
    }
}
