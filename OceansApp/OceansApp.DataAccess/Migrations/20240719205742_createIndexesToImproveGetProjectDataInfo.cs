using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class createIndexesToImproveGetProjectDataInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ProjectConsultantAssignedId_ActionDate",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY");

            migrationBuilder.CreateIndex(
                name: "IX_UserCategories_UserCategoryId",
                table: "UserCategories",
                column: "UserCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ProjectConsultantAssignedId_ActionDate_Id",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY",
                columns: new[] { "ProjectConsultantAssignedId", "ActionDate", "Id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserCategories_UserCategoryId",
                table: "UserCategories");

            migrationBuilder.DropIndex(
                name: "IX_PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ProjectConsultantAssignedId_ActionDate_Id",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY");

            migrationBuilder.CreateIndex(
                name: "IX_PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ProjectConsultantAssignedId_ActionDate",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY",
                columns: new[] { "ProjectConsultantAssignedId", "ActionDate" });
        }
    }
}
