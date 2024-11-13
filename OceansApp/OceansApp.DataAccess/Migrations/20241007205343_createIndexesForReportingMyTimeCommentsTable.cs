using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class createIndexesForReportingMyTimeCommentsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_REPORTING_MY_TIME_COMMENTS_ActionDate",
                table: "REPORTING_MY_TIME_COMMENTS",
                column: "ActionDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_REPORTING_MY_TIME_COMMENTS_ActionDate",
                table: "REPORTING_MY_TIME_COMMENTS");
        }
    }
}
