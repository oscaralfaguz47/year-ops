using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class addPrimaryAndSecondTrackingToolNameToMovementBlobsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PrimaryReportTrackingToolName",
                table: "REPORTING_MY_TIME_MOVEMENT_BLOBS",
                type: "varchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SecondReportTrackingToolName",
                table: "REPORTING_MY_TIME_MOVEMENT_BLOBS",
                type: "varchar(30)",
                maxLength: 30,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrimaryReportTrackingToolName",
                table: "REPORTING_MY_TIME_MOVEMENT_BLOBS");

            migrationBuilder.DropColumn(
                name: "SecondReportTrackingToolName",
                table: "REPORTING_MY_TIME_MOVEMENT_BLOBS");
        }
    }
}
