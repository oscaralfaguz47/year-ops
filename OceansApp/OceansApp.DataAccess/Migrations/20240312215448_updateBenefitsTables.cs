using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    public partial class updateBenefitsTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CONSULTANT_BENEFITS_ACCOUNTING_ACCOUNT_AccountingAccountId",
                table: "CONSULTANT_BENEFITS");

            migrationBuilder.DropForeignKey(
                name: "FK_CONSULTANT_BENEFITS_COST_CENTER_CostCenterId",
                table: "CONSULTANT_BENEFITS");

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

            migrationBuilder.DropIndex(
                name: "IX_CONSULTANT_BENEFITS_AccountingAccountId",
                table: "CONSULTANT_BENEFITS");

            migrationBuilder.DropIndex(
                name: "IX_CONSULTANT_BENEFITS_CostCenterId",
                table: "CONSULTANT_BENEFITS");

            migrationBuilder.DropColumn(
                name: "AccountingAccountId",
                table: "CONSULTANT_REIMBURSED_BENEFITS");

            migrationBuilder.DropColumn(
                name: "CostCenterId",
                table: "CONSULTANT_REIMBURSED_BENEFITS");

            migrationBuilder.DropColumn(
                name: "AccountingAccountId",
                table: "CONSULTANT_BENEFITS");

            migrationBuilder.DropColumn(
                name: "CostCenterId",
                table: "CONSULTANT_BENEFITS");

            migrationBuilder.CreateTable(
                name: "ConsultantBenefitCompany",
                columns: table => new
                {
                    ConsultantaBenefitCompanyId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    CostCenterId = table.Column<int>(type: "int", nullable: false),
                    AccountingAccountId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsultantBenefitCompany", x => x.ConsultantaBenefitCompanyId);
                    table.ForeignKey(
                        name: "FK_ConsultantBenefitCompany_ACCOUNTING_ACCOUNT_AccountingAccountId",
                        column: x => x.AccountingAccountId,
                        principalTable: "ACCOUNTING_ACCOUNT",
                        principalColumn: "AccountingAccountId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConsultantBenefitCompany_COST_CENTER_CostCenterId",
                        column: x => x.CostCenterId,
                        principalTable: "COST_CENTER",
                        principalColumn: "CostCenterId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConsultantBenefitCompany_AccountingAccountId",
                table: "ConsultantBenefitCompany",
                column: "AccountingAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsultantBenefitCompany_CostCenterId",
                table: "ConsultantBenefitCompany",
                column: "CostCenterId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConsultantBenefitCompany");

            migrationBuilder.AddColumn<int>(
                name: "AccountingAccountId",
                table: "CONSULTANT_REIMBURSED_BENEFITS",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CostCenterId",
                table: "CONSULTANT_REIMBURSED_BENEFITS",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "AccountingAccountId",
                table: "CONSULTANT_BENEFITS",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CostCenterId",
                table: "CONSULTANT_BENEFITS",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANT_REIMBURSED_BENEFITS_AccountingAccountId",
                table: "CONSULTANT_REIMBURSED_BENEFITS",
                column: "AccountingAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANT_REIMBURSED_BENEFITS_CostCenterId",
                table: "CONSULTANT_REIMBURSED_BENEFITS",
                column: "CostCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANT_BENEFITS_AccountingAccountId",
                table: "CONSULTANT_BENEFITS",
                column: "AccountingAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANT_BENEFITS_CostCenterId",
                table: "CONSULTANT_BENEFITS",
                column: "CostCenterId");

            migrationBuilder.AddForeignKey(
                name: "FK_CONSULTANT_BENEFITS_ACCOUNTING_ACCOUNT_AccountingAccountId",
                table: "CONSULTANT_BENEFITS",
                column: "AccountingAccountId",
                principalTable: "ACCOUNTING_ACCOUNT",
                principalColumn: "AccountingAccountId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CONSULTANT_BENEFITS_COST_CENTER_CostCenterId",
                table: "CONSULTANT_BENEFITS",
                column: "CostCenterId",
                principalTable: "COST_CENTER",
                principalColumn: "CostCenterId",
                onDelete: ReferentialAction.Cascade);

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
    }
}
