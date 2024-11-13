using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class createIndexesForPerformanceQuery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANT_REIMBURSED_BENEFITS_ConsultantId_TransactionStatusId_BenefitId",
                table: "CONSULTANT_REIMBURSED_BENEFITS",
                columns: new[] { "ConsultantId", "TransactionStatusId", "BenefitId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CONSULTANT_REIMBURSED_BENEFITS_ConsultantId_TransactionStatusId_BenefitId",
                table: "CONSULTANT_REIMBURSED_BENEFITS");
        }
    }
}
