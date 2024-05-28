using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    public partial class addCostCenterAndAccountingAccountToConsultantReimbursedBenefit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AccountingAccountId",
                table: "CONSULTANT_REIMBURSED_BENEFITS",
                type: "int",
                nullable: false);

            migrationBuilder.AddColumn<int>(
                name: "CostCenterId",
                table: "CONSULTANT_REIMBURSED_BENEFITS",
                type: "int",
                nullable: false);

            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANT_REIMBURSED_BENEFITS_AccountingAccountId",
                table: "CONSULTANT_REIMBURSED_BENEFITS",
                column: "AccountingAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANT_REIMBURSED_BENEFITS_CostCenterId",
                table: "CONSULTANT_REIMBURSED_BENEFITS",
                column: "CostCenterId");

            migrationBuilder.AddForeignKey(
                name: "FK_CONSULTANT_REIMBURSED_BENEFITS_ACCOUNTING_ACCOUNT_AccountingAccountId",
                table: "CONSULTANT_REIMBURSED_BENEFITS",
                column: "AccountingAccountId",
                principalTable: "ACCOUNTING_ACCOUNT",
                principalColumn: "AccountingAccountId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CONSULTANT_REIMBURSED_BENEFITS_COST_CENTER_CostCenterId",
                table: "CONSULTANT_REIMBURSED_BENEFITS",
                column: "CostCenterId",
                principalTable: "COST_CENTER",
                principalColumn: "CostCenterId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CONSULTANT_REIMBURSED_BENEFITS_ACCOUNTING_ACCOUNT_AccountingAccountId",
                table: "CONSULTANT_REIMBURSED_BENEFITS");

            migrationBuilder.DropForeignKey(
                name: "FK_CONSULTANT_REIMBURSED_BENEFITS_COST_CENTER_CostCenterId",
                table: "CONSULTANT_REIMBURSED_BENEFITS");

            migrationBuilder.DropIndex(
                name: "IX_CONSULTANT_REIMBURSED_BENEFITS_AccountingAccountId",
                table: "CONSULTANT_REIMBURSED_BENEFITS");

            migrationBuilder.DropIndex(
                name: "IX_CONSULTANT_REIMBURSED_BENEFITS_CostCenterId",
                table: "CONSULTANT_REIMBURSED_BENEFITS");

            migrationBuilder.DropColumn(
                name: "AccountingAccountId",
                table: "CONSULTANT_REIMBURSED_BENEFITS");

            migrationBuilder.DropColumn(
                name: "CostCenterId",
                table: "CONSULTANT_REIMBURSED_BENEFITS");
        }
    }
}
