using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class createIndexForHistoryActionsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS_ActionId",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS",
                column: "ActionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS_ActionId",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS");
        }
    }
}
