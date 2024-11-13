using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class createIndexesForInterviewsToImprovePerformance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_INTERVIEWS_ConsultantId_Date_TransactionStatusId",
                table: "INTERVIEWS",
                columns: new[] { "ConsultantId", "Date", "TransactionStatusId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_INTERVIEWS_ConsultantId_Date_TransactionStatusId",
                table: "INTERVIEWS");
        }
    }
}
