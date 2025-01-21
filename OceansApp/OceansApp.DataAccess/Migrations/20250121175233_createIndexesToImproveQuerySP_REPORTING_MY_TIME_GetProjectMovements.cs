using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class createIndexesToImproveQuerySP_REPORTING_MY_TIME_GetProjectMovements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ProjectConsultantsAssignedHistory_ProjectConsultantAssignedId_ActionDate_Id",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY",
                columns: new[] { "ProjectConsultantAssignedId", "ActionDate", "Id" },
                descending: new[] { false, true, true });

            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANT_DETAILS_ConsultantId_ConsultantHolidayId",
                table: "CONSULTANT_DETAILS",
                columns: new[] { "ConsultantId", "ConsultantHolidayId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProjectConsultantsAssignedHistory_ProjectConsultantAssignedId_ActionDate_Id",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY");

            migrationBuilder.DropIndex(
                name: "IX_CONSULTANT_DETAILS_ConsultantId_ConsultantHolidayId",
                table: "CONSULTANT_DETAILS");
        }
    }
}
