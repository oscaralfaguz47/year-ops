using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class updateBankAccuntsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BANK_ACCOUNTS_ACCOUNTING_ACCOUNT_AccountingAccountId",
                table: "BANK_ACCOUNTS");

            migrationBuilder.DropForeignKey(
                name: "FK_BANK_ACCOUNTS_COST_CENTER_CostCenterId",
                table: "BANK_ACCOUNTS");

            migrationBuilder.DropIndex(
                name: "IX_BANK_ACCOUNTS_AccountingAccountId",
                table: "BANK_ACCOUNTS");

            migrationBuilder.DropIndex(
                name: "IX_BANK_ACCOUNTS_CostCenterId",
                table: "BANK_ACCOUNTS");

            migrationBuilder.DropColumn(
                name: "AccountingAccountId",
                table: "BANK_ACCOUNTS");

            migrationBuilder.DropColumn(
                name: "CostCenterId",
                table: "BANK_ACCOUNTS");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AccountingAccountId",
                table: "BANK_ACCOUNTS",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CostCenterId",
                table: "BANK_ACCOUNTS",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_BANK_ACCOUNTS_AccountingAccountId",
                table: "BANK_ACCOUNTS",
                column: "AccountingAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_BANK_ACCOUNTS_CostCenterId",
                table: "BANK_ACCOUNTS",
                column: "CostCenterId");

            migrationBuilder.AddForeignKey(
                name: "FK_BANK_ACCOUNTS_ACCOUNTING_ACCOUNT_AccountingAccountId",
                table: "BANK_ACCOUNTS",
                column: "AccountingAccountId",
                principalTable: "ACCOUNTING_ACCOUNT",
                principalColumn: "AccountingAccountId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BANK_ACCOUNTS_COST_CENTER_CostCenterId",
                table: "BANK_ACCOUNTS",
                column: "CostCenterId",
                principalTable: "COST_CENTER",
                principalColumn: "CostCenterId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
