using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class createIndexForProjectConsultantAssigned : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_PROJECTS_CONSULTANTS_ASSIGNED_ProjectId_ConsultantId",
                table: "PROJECTS_CONSULTANTS_ASSIGNED",
                columns: new[] { "ProjectId", "ConsultantId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PROJECTS_CONSULTANTS_ASSIGNED_ProjectId_ConsultantId",
                table: "PROJECTS_CONSULTANTS_ASSIGNED");
        }
    }
}
