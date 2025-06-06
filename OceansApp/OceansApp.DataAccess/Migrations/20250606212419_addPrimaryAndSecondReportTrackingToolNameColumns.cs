using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class addPrimaryAndSecondReportTrackingToolNameColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PrimaryReportTrackingToolName",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY",
                type: "varchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SecondReportTrackingToolName",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY",
                type: "varchar(30)",
                maxLength: 30,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrimaryReportTrackingToolName",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY");

            migrationBuilder.DropColumn(
                name: "SecondReportTrackingToolName",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY");
        }
    }
}
