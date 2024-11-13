using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class createIndexesToImprovePerformanceDebitsCreditsQuery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Users_Name_LastName",
                table: "Users",
                columns: new[] { "Name", "LastName" });

            migrationBuilder.CreateIndex(
                name: "IX_TRANSACTION_TYPES_TransactionTypeId",
                table: "TRANSACTION_TYPES",
                column: "TransactionTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_TRANSACTION_TYPES_TransactionTypeId_Name",
                table: "TRANSACTION_TYPES",
                columns: new[] { "TransactionTypeId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_COST_CENTER_CostCenterId_Description",
                table: "COST_CENTER",
                columns: new[] { "CostCenterId", "Description" });

            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANT_PAYMENTS_DEBITS_CREDITS_ConsultantId_TransactionStatusId_TransactionTypeId_ActionDateWithinFortnight",
                table: "CONSULTANT_PAYMENTS_DEBITS_CREDITS",
                columns: new[] { "ConsultantId", "TransactionStatusId", "TransactionTypeId", "ActionDateWithinFortnight" });

            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANT_PAYMENTS_DEBITS_CREDITS_CreationDate_LastUpdateDate",
                table: "CONSULTANT_PAYMENTS_DEBITS_CREDITS",
                columns: new[] { "CreationDate", "LastUpdateDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ACCOUNTING_ACCOUNT_AccountingAccountCode",
                table: "ACCOUNTING_ACCOUNT",
                column: "AccountingAccountCode");

            migrationBuilder.CreateIndex(
                name: "IX_ACCOUNTING_ACCOUNT_AccountingAccountId",
                table: "ACCOUNTING_ACCOUNT",
                column: "AccountingAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_ACCOUNTING_ACCOUNT_AccountingAccountId_Description",
                table: "ACCOUNTING_ACCOUNT",
                columns: new[] { "AccountingAccountId", "Description" });

            migrationBuilder.CreateIndex(
                name: "IX_ACCOUNTING_ACCOUNT_CompanyId",
                table: "ACCOUNTING_ACCOUNT",
                column: "CompanyId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_Name_LastName",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_TRANSACTION_TYPES_TransactionTypeId",
                table: "TRANSACTION_TYPES");

            migrationBuilder.DropIndex(
                name: "IX_TRANSACTION_TYPES_TransactionTypeId_Name",
                table: "TRANSACTION_TYPES");

            migrationBuilder.DropIndex(
                name: "IX_COST_CENTER_CostCenterId_Description",
                table: "COST_CENTER");

            migrationBuilder.DropIndex(
                name: "IX_CONSULTANT_PAYMENTS_DEBITS_CREDITS_ConsultantId_TransactionStatusId_TransactionTypeId_ActionDateWithinFortnight",
                table: "CONSULTANT_PAYMENTS_DEBITS_CREDITS");

            migrationBuilder.DropIndex(
                name: "IX_CONSULTANT_PAYMENTS_DEBITS_CREDITS_CreationDate_LastUpdateDate",
                table: "CONSULTANT_PAYMENTS_DEBITS_CREDITS");

            migrationBuilder.DropIndex(
                name: "IX_ACCOUNTING_ACCOUNT_AccountingAccountCode",
                table: "ACCOUNTING_ACCOUNT");

            migrationBuilder.DropIndex(
                name: "IX_ACCOUNTING_ACCOUNT_AccountingAccountId",
                table: "ACCOUNTING_ACCOUNT");

            migrationBuilder.DropIndex(
                name: "IX_ACCOUNTING_ACCOUNT_AccountingAccountId_Description",
                table: "ACCOUNTING_ACCOUNT");

            migrationBuilder.DropIndex(
                name: "IX_ACCOUNTING_ACCOUNT_CompanyId",
                table: "ACCOUNTING_ACCOUNT");
        }
    }
}
