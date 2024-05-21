using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class addAccessToTrackingToolColumnToProjectConsultantAssigned : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AccessToTrackingTool",
                table: "PROJECTS_CONSULTANTS_ASSIGNED",
                type: "bit",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccessToTrackingTool",
                table: "PROJECTS_CONSULTANTS_ASSIGNED");
        }
    }
}
