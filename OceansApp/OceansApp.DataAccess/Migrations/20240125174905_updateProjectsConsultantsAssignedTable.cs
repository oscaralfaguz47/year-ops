using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    public partial class updateProjectsConsultantsAssignedTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "HourlyClientRate",
                table: "PROJECTS_CONSULTANTS_ASSIGNED",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "HourlySalary",
                table: "PROJECTS_CONSULTANTS_ASSIGNED",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsTheMontlyClientRateCalculatePerHour",
                table: "PROJECTS_CONSULTANTS_ASSIGNED",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "MonthlyClientRate",
                table: "PROJECTS_CONSULTANTS_ASSIGNED",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MonthlySalary",
                table: "PROJECTS_CONSULTANTS_ASSIGNED",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PositionDetail",
                table: "PROJECTS_CONSULTANTS_ASSIGNED",
                type: "nvarchar(130)",
                maxLength: 130,
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HourlyClientRate",
                table: "PROJECTS_CONSULTANTS_ASSIGNED");

            migrationBuilder.DropColumn(
                name: "HourlySalary",
                table: "PROJECTS_CONSULTANTS_ASSIGNED");

            migrationBuilder.DropColumn(
                name: "IsTheMontlyClientRateCalculatePerHour",
                table: "PROJECTS_CONSULTANTS_ASSIGNED");

            migrationBuilder.DropColumn(
                name: "MonthlyClientRate",
                table: "PROJECTS_CONSULTANTS_ASSIGNED");

            migrationBuilder.DropColumn(
                name: "MonthlySalary",
                table: "PROJECTS_CONSULTANTS_ASSIGNED");

            migrationBuilder.DropColumn(
                name: "PositionDetail",
                table: "PROJECTS_CONSULTANTS_ASSIGNED");
        }
    }
}
