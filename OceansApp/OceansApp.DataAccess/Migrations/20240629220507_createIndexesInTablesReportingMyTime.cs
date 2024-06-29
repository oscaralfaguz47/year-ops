using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class createIndexesInTablesReportingMyTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_REPORTING_MY_TIME_MOVEMENTS_ProjectId_ConsultantId_ActionDate",
                table: "REPORTING_MY_TIME_MOVEMENTS",
                columns: new[] { "ProjectId", "ConsultantId", "ActionDate" });

            migrationBuilder.CreateIndex(
                name: "IX_REPORTING_MY_TIME_MOVEMENT_TYPES_MovementTypeId_IsPayable",
                table: "REPORTING_MY_TIME_MOVEMENT_TYPES",
                columns: new[] { "MovementTypeId", "IsPayable" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_REPORTING_MY_TIME_MOVEMENTS_ProjectId_ConsultantId_ActionDate",
                table: "REPORTING_MY_TIME_MOVEMENTS");

            migrationBuilder.DropIndex(
                name: "IX_REPORTING_MY_TIME_MOVEMENT_TYPES_MovementTypeId_IsPayable",
                table: "REPORTING_MY_TIME_MOVEMENT_TYPES");
        }
    }
}
