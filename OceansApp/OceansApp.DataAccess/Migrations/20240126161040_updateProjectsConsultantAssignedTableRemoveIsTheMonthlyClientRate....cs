using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    public partial class updateProjectsConsultantAssignedTableRemoveIsTheMonthlyClientRate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsTheMontlyClientRateCalculatePerHour",
                table: "PROJECTS_CONSULTANTS_ASSIGNED");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsTheMontlyClientRateCalculatePerHour",
                table: "PROJECTS_CONSULTANTS_ASSIGNED",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
