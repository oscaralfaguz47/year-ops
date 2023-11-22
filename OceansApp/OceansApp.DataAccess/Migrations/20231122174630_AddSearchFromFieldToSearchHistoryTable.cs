using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    public partial class AddSearchFromFieldToSearchHistoryTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SearchFrom",
                table: "CALCULATOR_SEARCH_HISTORY",
                type: "nvarchar(35)",
                maxLength: 35,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SearchFrom",
                table: "CALCULATOR_SEARCH_HISTORY");
        }
    }
}
