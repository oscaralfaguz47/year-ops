using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class createIndexesForTablesImprovePerformance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_PROJECTS_CONSULTANTS_PENDING_SUBMISSIONS_ConsultantId_EndDate",
                table: "PROJECTS_CONSULTANTS_PENDING_SUBMISSIONS",
                columns: new[] { "ConsultantId", "EndDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PROJECTS_CONSULTANTS_PENDING_SUBMISSIONS_ConsultantId_EndDate",
                table: "PROJECTS_CONSULTANTS_PENDING_SUBMISSIONS");
        }
    }
}
