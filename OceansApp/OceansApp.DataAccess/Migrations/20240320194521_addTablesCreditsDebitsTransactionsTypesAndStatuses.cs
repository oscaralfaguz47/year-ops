using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    public partial class addTablesCreditsDebitsTransactionsTypesAndStatuses : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BenefitPaid",
                table: "CONSULTANT_REIMBURSED_BENEFITS");

            migrationBuilder.AddColumn<int>(
                name: "TransactionStatusId",
                table: "CONSULTANT_REIMBURSED_BENEFITS",
                type: "int",
                nullable: false);

            migrationBuilder.CreateTable(
                name: "TRANSACTION_STATUSES",
                columns: table => new
                {
                    TransactionStatusId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TRANSACTION_STATUSES", x => x.TransactionStatusId);
                });

            migrationBuilder.CreateTable(
                name: "TRANSACTION_TYPES",
                columns: table => new
                {
                    TransactionTypeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TRANSACTION_TYPES", x => x.TransactionTypeId);
                });

            migrationBuilder.CreateTable(
                name: "CONSULTANT_PAYMENTS_DEBITS_CREDITS",
                columns: table => new
                {
                    ConsultantPaymentDebitsCreditsId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConsultantId = table.Column<int>(type: "int", nullable: false),
                    AccountingAccountId = table.Column<int>(type: "int", nullable: false),
                    CostCenterId = table.Column<int>(type: "int", nullable: false),
                    Detail = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ActionDateWithinFortnight = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TransactionStatusId = table.Column<int>(type: "int", nullable: false),
                    TransactionTypeId = table.Column<int>(type: "int", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ConsultantIdCreatedBy = table.Column<int>(type: "int", nullable: false),
                    LastUpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ConsultantIdLastUpdatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CONSULTANT_PAYMENTS_DEBITS_CREDITS", x => x.ConsultantPaymentDebitsCreditsId);
                    table.ForeignKey(
                        name: "FK_CONSULTANT_PAYMENTS_DEBITS_CREDITS_ACCOUNTING_ACCOUNT_AccountingAccountId",
                        column: x => x.AccountingAccountId,
                        principalTable: "ACCOUNTING_ACCOUNT",
                        principalColumn: "AccountingAccountId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CONSULTANT_PAYMENTS_DEBITS_CREDITS_CONSULTANT_DETAILS_ConsultantId",
                        column: x => x.ConsultantId,
                        principalTable: "CONSULTANT_DETAILS",
                        principalColumn: "ConsultantId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CONSULTANT_PAYMENTS_DEBITS_CREDITS_CONSULTANT_DETAILS_ConsultantIdCreatedBy",
                        column: x => x.ConsultantIdCreatedBy,
                        principalTable: "CONSULTANT_DETAILS",
                        principalColumn: "ConsultantId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CONSULTANT_PAYMENTS_DEBITS_CREDITS_CONSULTANT_DETAILS_ConsultantIdLastUpdatedBy",
                        column: x => x.ConsultantIdLastUpdatedBy,
                        principalTable: "CONSULTANT_DETAILS",
                        principalColumn: "ConsultantId");
                    table.ForeignKey(
                        name: "FK_CONSULTANT_PAYMENTS_DEBITS_CREDITS_COST_CENTER_CostCenterId",
                        column: x => x.CostCenterId,
                        principalTable: "COST_CENTER",
                        principalColumn: "CostCenterId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CONSULTANT_PAYMENTS_DEBITS_CREDITS_TRANSACTION_STATUSES_TransactionStatusId",
                        column: x => x.TransactionStatusId,
                        principalTable: "TRANSACTION_STATUSES",
                        principalColumn: "TransactionStatusId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CONSULTANT_PAYMENTS_DEBITS_CREDITS_TRANSACTION_TYPES_TransactionTypeId",
                        column: x => x.TransactionTypeId,
                        principalTable: "TRANSACTION_TYPES",
                        principalColumn: "TransactionTypeId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANT_REIMBURSED_BENEFITS_TransactionStatusId",
                table: "CONSULTANT_REIMBURSED_BENEFITS",
                column: "TransactionStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANT_PAYMENTS_DEBITS_CREDITS_AccountingAccountId",
                table: "CONSULTANT_PAYMENTS_DEBITS_CREDITS",
                column: "AccountingAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANT_PAYMENTS_DEBITS_CREDITS_ConsultantId",
                table: "CONSULTANT_PAYMENTS_DEBITS_CREDITS",
                column: "ConsultantId");

            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANT_PAYMENTS_DEBITS_CREDITS_ConsultantIdCreatedBy",
                table: "CONSULTANT_PAYMENTS_DEBITS_CREDITS",
                column: "ConsultantIdCreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANT_PAYMENTS_DEBITS_CREDITS_ConsultantIdLastUpdatedBy",
                table: "CONSULTANT_PAYMENTS_DEBITS_CREDITS",
                column: "ConsultantIdLastUpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANT_PAYMENTS_DEBITS_CREDITS_CostCenterId",
                table: "CONSULTANT_PAYMENTS_DEBITS_CREDITS",
                column: "CostCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANT_PAYMENTS_DEBITS_CREDITS_TransactionStatusId",
                table: "CONSULTANT_PAYMENTS_DEBITS_CREDITS",
                column: "TransactionStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANT_PAYMENTS_DEBITS_CREDITS_TransactionTypeId",
                table: "CONSULTANT_PAYMENTS_DEBITS_CREDITS",
                column: "TransactionTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_CONSULTANT_REIMBURSED_BENEFITS_TRANSACTION_STATUSES_TransactionStatusId",
                table: "CONSULTANT_REIMBURSED_BENEFITS",
                column: "TransactionStatusId",
                principalTable: "TRANSACTION_STATUSES",
                principalColumn: "TransactionStatusId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CONSULTANT_REIMBURSED_BENEFITS_TRANSACTION_STATUSES_TransactionStatusId",
                table: "CONSULTANT_REIMBURSED_BENEFITS");

            migrationBuilder.DropTable(
                name: "CONSULTANT_PAYMENTS_DEBITS_CREDITS");

            migrationBuilder.DropTable(
                name: "TRANSACTION_STATUSES");

            migrationBuilder.DropTable(
                name: "TRANSACTION_TYPES");

            migrationBuilder.DropIndex(
                name: "IX_CONSULTANT_REIMBURSED_BENEFITS_TransactionStatusId",
                table: "CONSULTANT_REIMBURSED_BENEFITS");

            migrationBuilder.DropColumn(
                name: "TransactionStatusId",
                table: "CONSULTANT_REIMBURSED_BENEFITS");

            migrationBuilder.AddColumn<bool>(
                name: "BenefitPaid",
                table: "CONSULTANT_REIMBURSED_BENEFITS",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
