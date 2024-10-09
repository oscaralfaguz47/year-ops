using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class createIndexesToImproveQuery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_IsActive_AccessToTrackingTool",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY",
                columns: new[] { "IsActive", "AccessToTrackingTool" });

            migrationBuilder.CreateIndex(
                name: "IX_PROJECTS_CONSULTANTS_ASSIGNED_ConsultantId_ProjectConsultantAssignedId",
                table: "PROJECTS_CONSULTANTS_ASSIGNED",
                columns: new[] { "ConsultantId", "ProjectConsultantAssignedId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_IsActive_AccessToTrackingTool",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY");

            migrationBuilder.DropIndex(
                name: "IX_PROJECTS_CONSULTANTS_ASSIGNED_ConsultantId_ProjectConsultantAssignedId",
                table: "PROJECTS_CONSULTANTS_ASSIGNED");
        }
    }
}
