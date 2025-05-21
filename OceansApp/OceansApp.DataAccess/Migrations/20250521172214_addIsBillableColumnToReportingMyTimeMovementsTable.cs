using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class addIsBillableColumnToReportingMyTimeMovementsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsBillable",
                table: "REPORTING_MY_TIME_MOVEMENTS",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateIndex(
                name: "IX_REPORTING_MY_TIME_MOVEMENTS_IsBillable",
                table: "REPORTING_MY_TIME_MOVEMENTS",
                column: "IsBillable");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_REPORTING_MY_TIME_MOVEMENTS_IsBillable",
                table: "REPORTING_MY_TIME_MOVEMENTS");

            migrationBuilder.DropColumn(
                name: "IsBillable",
                table: "REPORTING_MY_TIME_MOVEMENTS");
        }
    }
}
