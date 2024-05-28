using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class updateReportingMyTimeMovementBlobsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Metadata",
                table: "REPORTING_MY_TIME_MOVEMENT_BLOBS");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Metadata",
                table: "REPORTING_MY_TIME_MOVEMENT_BLOBS",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
