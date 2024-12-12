using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class createNewIndexesToImprovePerformance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANT_REIMBURSED_BENEFITS_ConsultantId_DateToBeReimbursed",
                table: "CONSULTANT_REIMBURSED_BENEFITS",
                columns: new[] { "ConsultantId", "DateToBeReimbursed" })
                .Annotation("SqlServer:Include", new[] { "TransactionStatusId", "BenefitId", "BenefitCategoryId", "AmountReimbursed" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CONSULTANT_REIMBURSED_BENEFITS_ConsultantId_DateToBeReimbursed",
                table: "CONSULTANT_REIMBURSED_BENEFITS");
        }
    }
}
