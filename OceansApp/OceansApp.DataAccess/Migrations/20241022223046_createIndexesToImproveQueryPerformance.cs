using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class createIndexesToImproveQueryPerformance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_COST_CENTER_CostCenterCode",
                table: "COST_CENTER",
                column: "CostCenterCode");

            migrationBuilder.CreateIndex(
                name: "IX_COST_CENTER_CostCenterId",
                table: "COST_CENTER",
                column: "CostCenterId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_COST_CENTER_CostCenterCode",
                table: "COST_CENTER");

            migrationBuilder.DropIndex(
                name: "IX_COST_CENTER_CostCenterId",
                table: "COST_CENTER");
        }
    }
}
