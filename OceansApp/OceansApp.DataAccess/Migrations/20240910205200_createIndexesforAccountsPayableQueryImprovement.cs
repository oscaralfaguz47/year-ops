using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class createIndexesforAccountsPayableQueryImprovement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_TRANSACTION_STATUSES_Name",
                table: "TRANSACTION_STATUSES",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_JOURNAL_ACCOUNTS_PAYABLE_CompanyId_TransactionStatusId",
                table: "JOURNAL_ACCOUNTS_PAYABLE",
                columns: new[] { "CompanyId", "TransactionStatusId" });

            migrationBuilder.CreateIndex(
                name: "IX_COMPANIES_Name",
                table: "COMPANIES",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TRANSACTION_STATUSES_Name",
                table: "TRANSACTION_STATUSES");

            migrationBuilder.DropIndex(
                name: "IX_JOURNAL_ACCOUNTS_PAYABLE_CompanyId_TransactionStatusId",
                table: "JOURNAL_ACCOUNTS_PAYABLE");

            migrationBuilder.DropIndex(
                name: "IX_COMPANIES_Name",
                table: "COMPANIES");
        }
    }
}
