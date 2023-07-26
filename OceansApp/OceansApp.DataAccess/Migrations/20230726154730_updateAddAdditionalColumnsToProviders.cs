using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    public partial class updateAddAdditionalColumnsToProviders : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ConsultantCategory",
                table: "PROVIDER",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "HourlyClientRate",
                table: "PROVIDER",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "HourlySalary",
                table: "PROVIDER",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsTheMonthlyClientRateCalculatePerHour",
                table: "PROVIDER",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "PROVIDER",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MonthlyClientRate",
                table: "PROVIDER",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MonthlySalary",
                table: "PROVIDER",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PersonalEmail",
                table: "PROVIDER",
                type: "nvarchar(249)",
                maxLength: 249,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShirtSize",
                table: "PROVIDER",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConsultantCategory",
                table: "PROVIDER");

            migrationBuilder.DropColumn(
                name: "HourlyClientRate",
                table: "PROVIDER");

            migrationBuilder.DropColumn(
                name: "HourlySalary",
                table: "PROVIDER");

            migrationBuilder.DropColumn(
                name: "IsTheMonthlyClientRateCalculatePerHour",
                table: "PROVIDER");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "PROVIDER");

            migrationBuilder.DropColumn(
                name: "MonthlyClientRate",
                table: "PROVIDER");

            migrationBuilder.DropColumn(
                name: "MonthlySalary",
                table: "PROVIDER");

            migrationBuilder.DropColumn(
                name: "PersonalEmail",
                table: "PROVIDER");

            migrationBuilder.DropColumn(
                name: "ShirtSize",
                table: "PROVIDER");
        }
    }
}
