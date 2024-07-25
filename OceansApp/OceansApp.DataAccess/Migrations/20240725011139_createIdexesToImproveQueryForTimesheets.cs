using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class createIdexesToImproveQueryForTimesheets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_PROJECTS_USERS_SELECTED_ProjectId",
                table: "PROJECTS_USERS_SELECTED",
                column: "ProjectId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PROJECTS_USERS_SELECTED_ProjectId",
                table: "PROJECTS_USERS_SELECTED");
        }
    }
}
