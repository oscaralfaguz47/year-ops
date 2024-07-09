using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class createIndexesForImproveSPGetProjectMovementsTrackingTool : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANT_HOLIDAYS_ConsultantHolidayId",
                table: "CONSULTANT_HOLIDAYS",
                column: "ConsultantHolidayId");

            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANT_DETAILS_ConsultantId",
                table: "CONSULTANT_DETAILS",
                column: "ConsultantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CONSULTANT_HOLIDAYS_ConsultantHolidayId",
                table: "CONSULTANT_HOLIDAYS");

            migrationBuilder.DropIndex(
                name: "IX_CONSULTANT_DETAILS_ConsultantId",
                table: "CONSULTANT_DETAILS");
        }
    }
}
