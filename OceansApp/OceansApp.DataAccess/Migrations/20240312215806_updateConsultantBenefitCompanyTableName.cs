using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    public partial class updateConsultantBenefitCompanyTableName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConsultantBenefitCompany_ACCOUNTING_ACCOUNT_AccountingAccountId",
                table: "ConsultantBenefitCompany");

            migrationBuilder.DropForeignKey(
                name: "FK_ConsultantBenefitCompany_COST_CENTER_CostCenterId",
                table: "ConsultantBenefitCompany");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ConsultantBenefitCompany",
                table: "ConsultantBenefitCompany");

            migrationBuilder.RenameTable(
                name: "ConsultantBenefitCompany",
                newName: "CONSULTANT_BENEFIT_COMPANIES");

            migrationBuilder.RenameIndex(
                name: "IX_ConsultantBenefitCompany_CostCenterId",
                table: "CONSULTANT_BENEFIT_COMPANIES",
                newName: "IX_CONSULTANT_BENEFIT_COMPANIES_CostCenterId");

            migrationBuilder.RenameIndex(
                name: "IX_ConsultantBenefitCompany_AccountingAccountId",
                table: "CONSULTANT_BENEFIT_COMPANIES",
                newName: "IX_CONSULTANT_BENEFIT_COMPANIES_AccountingAccountId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CONSULTANT_BENEFIT_COMPANIES",
                table: "CONSULTANT_BENEFIT_COMPANIES",
                column: "ConsultantaBenefitCompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_CONSULTANT_BENEFIT_COMPANIES_ACCOUNTING_ACCOUNT_AccountingAccountId",
                table: "CONSULTANT_BENEFIT_COMPANIES",
                column: "AccountingAccountId",
                principalTable: "ACCOUNTING_ACCOUNT",
                principalColumn: "AccountingAccountId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CONSULTANT_BENEFIT_COMPANIES_COST_CENTER_CostCenterId",
                table: "CONSULTANT_BENEFIT_COMPANIES",
                column: "CostCenterId",
                principalTable: "COST_CENTER",
                principalColumn: "CostCenterId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CONSULTANT_BENEFIT_COMPANIES_ACCOUNTING_ACCOUNT_AccountingAccountId",
                table: "CONSULTANT_BENEFIT_COMPANIES");

            migrationBuilder.DropForeignKey(
                name: "FK_CONSULTANT_BENEFIT_COMPANIES_COST_CENTER_CostCenterId",
                table: "CONSULTANT_BENEFIT_COMPANIES");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CONSULTANT_BENEFIT_COMPANIES",
                table: "CONSULTANT_BENEFIT_COMPANIES");

            migrationBuilder.RenameTable(
                name: "CONSULTANT_BENEFIT_COMPANIES",
                newName: "ConsultantBenefitCompany");

            migrationBuilder.RenameIndex(
                name: "IX_CONSULTANT_BENEFIT_COMPANIES_CostCenterId",
                table: "ConsultantBenefitCompany",
                newName: "IX_ConsultantBenefitCompany_CostCenterId");

            migrationBuilder.RenameIndex(
                name: "IX_CONSULTANT_BENEFIT_COMPANIES_AccountingAccountId",
                table: "ConsultantBenefitCompany",
                newName: "IX_ConsultantBenefitCompany_AccountingAccountId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ConsultantBenefitCompany",
                table: "ConsultantBenefitCompany",
                column: "ConsultantaBenefitCompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_ConsultantBenefitCompany_ACCOUNTING_ACCOUNT_AccountingAccountId",
                table: "ConsultantBenefitCompany",
                column: "AccountingAccountId",
                principalTable: "ACCOUNTING_ACCOUNT",
                principalColumn: "AccountingAccountId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ConsultantBenefitCompany_COST_CENTER_CostCenterId",
                table: "ConsultantBenefitCompany",
                column: "CostCenterId",
                principalTable: "COST_CENTER",
                principalColumn: "CostCenterId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
