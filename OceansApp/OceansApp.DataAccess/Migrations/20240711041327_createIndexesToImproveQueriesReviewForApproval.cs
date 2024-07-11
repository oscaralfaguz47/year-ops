using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class createIndexesToImproveQueriesReviewForApproval : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_REPORTING_MY_TIME_MOVEMENTS_SUBMISSIONS_ProjectId_ConsultantId",
                table: "REPORTING_MY_TIME_MOVEMENTS_SUBMISSIONS",
                columns: new[] { "ProjectId", "ConsultantId" });

            migrationBuilder.CreateIndex(
                name: "IX_REPORTING_MY_TIME_MOVEMENTS_SUBMISSIONS_SubmissionId",
                table: "REPORTING_MY_TIME_MOVEMENTS_SUBMISSIONS",
                column: "SubmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_REPORTING_MY_TIME_MOVEMENTS_ProjectId_ConsultantId",
                table: "REPORTING_MY_TIME_MOVEMENTS",
                columns: new[] { "ProjectId", "ConsultantId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_REPORTING_MY_TIME_MOVEMENTS_SUBMISSIONS_ProjectId_ConsultantId",
                table: "REPORTING_MY_TIME_MOVEMENTS_SUBMISSIONS");

            migrationBuilder.DropIndex(
                name: "IX_REPORTING_MY_TIME_MOVEMENTS_SUBMISSIONS_SubmissionId",
                table: "REPORTING_MY_TIME_MOVEMENTS_SUBMISSIONS");

            migrationBuilder.DropIndex(
                name: "IX_REPORTING_MY_TIME_MOVEMENTS_ProjectId_ConsultantId",
                table: "REPORTING_MY_TIME_MOVEMENTS");
        }
    }
}
