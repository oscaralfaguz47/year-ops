using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class addNumHoursForHolidayColumnInProjectConsultantAssignedHistoryTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NumHoursForHoliday",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY",
                type: "int",
                nullable: false,
                defaultValue: 8);

            migrationBuilder.CreateIndex(
                name: "IX_PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_NumHoursForHoliday",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY",
                column: "NumHoursForHoliday");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_NumHoursForHoliday",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY");

            migrationBuilder.DropColumn(
                name: "NumHoursForHoliday",
                table: "PROJECTS_CONSULTANTS_ASSIGNED_HISTORY");
        }
    }
}
