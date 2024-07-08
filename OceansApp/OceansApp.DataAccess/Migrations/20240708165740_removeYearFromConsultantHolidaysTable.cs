using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class removeYearFromConsultantHolidaysTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CONSULTANT_HOLIDAYS_Year",
                table: "CONSULTANT_HOLIDAYS");

            migrationBuilder.DropColumn(
                name: "Year",
                table: "CONSULTANT_HOLIDAYS");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Year",
                table: "CONSULTANT_HOLIDAYS",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANT_HOLIDAYS_Year",
                table: "CONSULTANT_HOLIDAYS",
                column: "Year");
        }
    }
}
