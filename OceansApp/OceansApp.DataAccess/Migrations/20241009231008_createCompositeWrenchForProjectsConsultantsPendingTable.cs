using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class createCompositeWrenchForProjectsConsultantsPendingTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PROJECTS_CONSULTANTS_PENDING_SUBMISSIONS_ProjectId_ConsultantId_StartDate_EndDate",
                table: "PROJECTS_CONSULTANTS_PENDING_SUBMISSIONS");

            migrationBuilder.CreateIndex(
                name: "IX_PROJECTS_CONSULTANTS_PENDING_SUBMISSIONS_ProjectId_ConsultantId_StartDate_EndDate",
                table: "PROJECTS_CONSULTANTS_PENDING_SUBMISSIONS",
                columns: new[] { "ProjectId", "ConsultantId", "StartDate", "EndDate" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PROJECTS_CONSULTANTS_PENDING_SUBMISSIONS_ProjectId_ConsultantId_StartDate_EndDate",
                table: "PROJECTS_CONSULTANTS_PENDING_SUBMISSIONS");

            migrationBuilder.CreateIndex(
                name: "IX_PROJECTS_CONSULTANTS_PENDING_SUBMISSIONS_ProjectId_ConsultantId_StartDate_EndDate",
                table: "PROJECTS_CONSULTANTS_PENDING_SUBMISSIONS",
                columns: new[] { "ProjectId", "ConsultantId", "StartDate", "EndDate" });
        }
    }
}
