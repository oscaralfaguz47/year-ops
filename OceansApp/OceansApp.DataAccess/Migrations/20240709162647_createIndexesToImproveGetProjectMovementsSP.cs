using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class createIndexesToImproveGetProjectMovementsSP : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANT_HOLIDAY_DATES_ConsultantHolidayId_Date",
                table: "CONSULTANT_HOLIDAY_DATES",
                columns: new[] { "ConsultantHolidayId", "Date" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CONSULTANT_HOLIDAY_DATES_ConsultantHolidayId_Date",
                table: "CONSULTANT_HOLIDAY_DATES");
        }
    }
}
