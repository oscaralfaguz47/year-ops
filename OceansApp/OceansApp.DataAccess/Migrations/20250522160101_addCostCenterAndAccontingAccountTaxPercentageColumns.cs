using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class addCostCenterAndAccontingAccountTaxPercentageColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AccountingAccountIdTaxPercentage",
                table: "PRODUCTS_CLIENTS_COMPANIES_ACCOUNTING_CONFIG",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CostCenterIdTaxPercentage",
                table: "PRODUCTS_CLIENTS_COMPANIES_ACCOUNTING_CONFIG",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PRODUCTS_CLIENTS_COMPANIES_ACCOUNTING_CONFIG_AccountingAccountIdTaxPercentage",
                table: "PRODUCTS_CLIENTS_COMPANIES_ACCOUNTING_CONFIG",
                column: "AccountingAccountIdTaxPercentage");

            migrationBuilder.CreateIndex(
                name: "IX_PRODUCTS_CLIENTS_COMPANIES_ACCOUNTING_CONFIG_CostCenterIdTaxPercentage",
                table: "PRODUCTS_CLIENTS_COMPANIES_ACCOUNTING_CONFIG",
                column: "CostCenterIdTaxPercentage");

            migrationBuilder.AddForeignKey(
                name: "FK_PRODUCTS_CLIENTS_COMPANIES_ACCOUNTING_CONFIG_ACCOUNTING_ACCOUNT_AccountingAccountIdTaxPercentage",
                table: "PRODUCTS_CLIENTS_COMPANIES_ACCOUNTING_CONFIG",
                column: "AccountingAccountIdTaxPercentage",
                principalTable: "ACCOUNTING_ACCOUNT",
                principalColumn: "AccountingAccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_PRODUCTS_CLIENTS_COMPANIES_ACCOUNTING_CONFIG_COST_CENTER_CostCenterIdTaxPercentage",
                table: "PRODUCTS_CLIENTS_COMPANIES_ACCOUNTING_CONFIG",
                column: "CostCenterIdTaxPercentage",
                principalTable: "COST_CENTER",
                principalColumn: "CostCenterId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PRODUCTS_CLIENTS_COMPANIES_ACCOUNTING_CONFIG_ACCOUNTING_ACCOUNT_AccountingAccountIdTaxPercentage",
                table: "PRODUCTS_CLIENTS_COMPANIES_ACCOUNTING_CONFIG");

            migrationBuilder.DropForeignKey(
                name: "FK_PRODUCTS_CLIENTS_COMPANIES_ACCOUNTING_CONFIG_COST_CENTER_CostCenterIdTaxPercentage",
                table: "PRODUCTS_CLIENTS_COMPANIES_ACCOUNTING_CONFIG");

            migrationBuilder.DropIndex(
                name: "IX_PRODUCTS_CLIENTS_COMPANIES_ACCOUNTING_CONFIG_AccountingAccountIdTaxPercentage",
                table: "PRODUCTS_CLIENTS_COMPANIES_ACCOUNTING_CONFIG");

            migrationBuilder.DropIndex(
                name: "IX_PRODUCTS_CLIENTS_COMPANIES_ACCOUNTING_CONFIG_CostCenterIdTaxPercentage",
                table: "PRODUCTS_CLIENTS_COMPANIES_ACCOUNTING_CONFIG");

            migrationBuilder.DropColumn(
                name: "AccountingAccountIdTaxPercentage",
                table: "PRODUCTS_CLIENTS_COMPANIES_ACCOUNTING_CONFIG");

            migrationBuilder.DropColumn(
                name: "CostCenterIdTaxPercentage",
                table: "PRODUCTS_CLIENTS_COMPANIES_ACCOUNTING_CONFIG");
        }
    }
}
