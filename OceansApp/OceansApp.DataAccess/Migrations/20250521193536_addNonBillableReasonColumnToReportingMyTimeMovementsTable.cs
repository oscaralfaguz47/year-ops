using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class addNonBillableReasonColumnToReportingMyTimeMovementsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NonBillableReason",
                table: "REPORTING_MY_TIME_MOVEMENTS",
                type: "varchar(800)",
                maxLength: 800,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NonBillableReason",
                table: "REPORTING_MY_TIME_MOVEMENTS");
        }
    }
}
