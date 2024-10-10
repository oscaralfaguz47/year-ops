using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class createIndexesToImprovePerformanceInSQL : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_PROJECTS_CONSULTANTS_PENDING_SUBMISSIONS_ProjectId_ConsultantId_StartDate_EndDate",
                table: "PROJECTS_CONSULTANTS_PENDING_SUBMISSIONS",
                columns: new[] { "ProjectId", "ConsultantId", "StartDate", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ProjectConsultantAssignedId_IsActive_MonthlySalary_HourlySalary",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY",
                columns: new[] { "ProjectConsultantAssignedId", "IsActive", "MonthlySalary", "HourlySalary" },
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_PROJECTS_CONSULTANTS_ASSIGNED_ConsultantId_ProjectId_ProjectConsultantAssignedId",
                table: "PROJECTS_CONSULTANTS_ASSIGNED",
                columns: new[] { "ConsultantId", "ProjectId", "ProjectConsultantAssignedId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PROJECTS_CONSULTANTS_PENDING_SUBMISSIONS_ProjectId_ConsultantId_StartDate_EndDate",
                table: "PROJECTS_CONSULTANTS_PENDING_SUBMISSIONS");

            migrationBuilder.DropIndex(
                name: "IX_PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ProjectConsultantAssignedId_IsActive_MonthlySalary_HourlySalary",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY");

            migrationBuilder.DropIndex(
                name: "IX_PROJECTS_CONSULTANTS_ASSIGNED_ConsultantId_ProjectId_ProjectConsultantAssignedId",
                table: "PROJECTS_CONSULTANTS_ASSIGNED");
        }
    }
}
