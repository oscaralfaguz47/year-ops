using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class createIndexesForReportingMyTimeSubmissionTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_REPORTING_MY_TIME_MOVEMENTS_SUBMISSIONS_ConsultantId_ProjectId_StartPeriodDate_EndPeriodDate",
                table: "REPORTING_MY_TIME_MOVEMENTS_SUBMISSIONS",
                columns: new[] { "ConsultantId", "ProjectId", "StartPeriodDate", "EndPeriodDate" });

            migrationBuilder.CreateIndex(
                name: "IX_REPORTING_MY_TIME_MOVEMENTS_SUBMISSIONS_LastSubmissionDate",
                table: "REPORTING_MY_TIME_MOVEMENTS_SUBMISSIONS",
                column: "LastSubmissionDate");

            migrationBuilder.CreateIndex(
                name: "IX_REPORTING_MY_TIME_MOVEMENTS_SUBMISSIONS_SubmissionDate",
                table: "REPORTING_MY_TIME_MOVEMENTS_SUBMISSIONS",
                column: "SubmissionDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_REPORTING_MY_TIME_MOVEMENTS_SUBMISSIONS_ConsultantId_ProjectId_StartPeriodDate_EndPeriodDate",
                table: "REPORTING_MY_TIME_MOVEMENTS_SUBMISSIONS");

            migrationBuilder.DropIndex(
                name: "IX_REPORTING_MY_TIME_MOVEMENTS_SUBMISSIONS_LastSubmissionDate",
                table: "REPORTING_MY_TIME_MOVEMENTS_SUBMISSIONS");

            migrationBuilder.DropIndex(
                name: "IX_REPORTING_MY_TIME_MOVEMENTS_SUBMISSIONS_SubmissionDate",
                table: "REPORTING_MY_TIME_MOVEMENTS_SUBMISSIONS");
        }
    }
}
