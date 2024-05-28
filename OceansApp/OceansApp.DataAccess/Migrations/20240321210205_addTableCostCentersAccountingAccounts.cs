using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    public partial class addTableCostCentersAccountingAccounts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "COSTS_CENTERS_ACCOUNTING_ACCOUNTS",
                columns: table => new
                {
                    CostCenterAccountingAccountId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CostCenterId = table.Column<int>(type: "int", nullable: false),
                    AccountingAccountId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompanyId = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_COSTS_CENTERS_ACCOUNTING_ACCOUNTS", x => x.CostCenterAccountingAccountId);
                    table.ForeignKey(
                        name: "FK_COSTS_CENTERS_ACCOUNTING_ACCOUNTS_ACCOUNTING_ACCOUNT_AccountingAccountId",
                        column: x => x.AccountingAccountId,
                        principalTable: "ACCOUNTING_ACCOUNT",
                        principalColumn: "AccountingAccountId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_COSTS_CENTERS_ACCOUNTING_ACCOUNTS_COST_CENTER_CostCenterId",
                        column: x => x.CostCenterId,
                        principalTable: "COST_CENTER",
                        principalColumn: "CostCenterId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_COSTS_CENTERS_ACCOUNTING_ACCOUNTS_AccountingAccountId",
                table: "COSTS_CENTERS_ACCOUNTING_ACCOUNTS",
                column: "AccountingAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_COSTS_CENTERS_ACCOUNTING_ACCOUNTS_CostCenterId",
                table: "COSTS_CENTERS_ACCOUNTING_ACCOUNTS",
                column: "CostCenterId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "COSTS_CENTERS_ACCOUNTING_ACCOUNTS");
        }
    }
}
