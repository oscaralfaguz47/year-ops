using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    public partial class addMinimumGlobalProfitToGlobalConfigTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "MinimumGlobalProfit",
                table: "CALCULATOR_GLOBAL_CONFIGURATIONS",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MinimumGlobalProfit",
                table: "CALCULATOR_GLOBAL_CONFIGURATIONS");
        }
    }
}
