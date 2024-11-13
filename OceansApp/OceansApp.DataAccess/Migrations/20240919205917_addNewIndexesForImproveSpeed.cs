using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class addNewIndexesForImproveSpeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_TRANSACTION_STATUSES_TransactionStatusId_Name",
                table: "TRANSACTION_STATUSES",
                columns: new[] { "TransactionStatusId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_ACCOUNTS_PAYABLE_ConsultantId_TransactionStatusId_StartDatePeriod_EndDatePeriod",
                table: "ACCOUNTS_PAYABLE",
                columns: new[] { "ConsultantId", "TransactionStatusId", "StartDatePeriod", "EndDatePeriod" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TRANSACTION_STATUSES_TransactionStatusId_Name",
                table: "TRANSACTION_STATUSES");

            migrationBuilder.DropIndex(
                name: "IX_ACCOUNTS_PAYABLE_ConsultantId_TransactionStatusId_StartDatePeriod_EndDatePeriod",
                table: "ACCOUNTS_PAYABLE");
        }
    }
}
