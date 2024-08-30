using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class createIndexForProjecConsultantAssignedTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_PROJECTS_CONSULTANTS_ASSIGNED_ProjectConsultantAssignedId",
                table: "PROJECTS_CONSULTANTS_ASSIGNED",
                column: "ProjectConsultantAssignedId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PROJECTS_CONSULTANTS_ASSIGNED_ProjectConsultantAssignedId",
                table: "PROJECTS_CONSULTANTS_ASSIGNED");
        }
    }
}
