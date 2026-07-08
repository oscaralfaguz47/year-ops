using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class DropKpiInScopeAddIncludeInReview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InScope",
                table: "KPI_DEFINITIONS");

            // DEFAULT TRUE: new results are included in the Review unless explicitly un-ticked,
            // and any existing results are backfilled to included.
            migrationBuilder.AddColumn<bool>(
                name: "IncludeInReview",
                table: "KPI_RESULTS",
                type: "bit",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IncludeInReview",
                table: "KPI_RESULTS");

            migrationBuilder.AddColumn<bool>(
                name: "InScope",
                table: "KPI_DEFINITIONS",
                type: "bit",
                nullable: false,
                defaultValue: true);
        }
    }
}
