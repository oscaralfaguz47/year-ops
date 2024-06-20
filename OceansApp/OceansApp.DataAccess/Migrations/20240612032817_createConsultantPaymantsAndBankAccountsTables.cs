using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class createConsultantPaymantsAndBankAccountsTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BANK_ACCOUNTS",
                columns: table => new
                {
                    BankAccountId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BankAccountCode = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    BankAccountName = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false),
                    IsActive = table.Column<string>(type: "varchar(1)", maxLength: 1, nullable: false),
                    CostCenterId = table.Column<int>(type: "int", nullable: false),
                    AccountingAccountId = table.Column<int>(type: "int", nullable: false),
                    CompanyId = table.Column<string>(type: "varchar(8)", maxLength: 8, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BANK_ACCOUNTS", x => x.BankAccountId);
                    table.ForeignKey(
                        name: "FK_BANK_ACCOUNTS_ACCOUNTING_ACCOUNT_AccountingAccountId",
                        column: x => x.AccountingAccountId,
                        principalTable: "ACCOUNTING_ACCOUNT",
                        principalColumn: "AccountingAccountId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BANK_ACCOUNTS_COST_CENTER_CostCenterId",
                        column: x => x.CostCenterId,
                        principalTable: "COST_CENTER",
                        principalColumn: "CostCenterId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CONSULTANT_PAYMENTS",
                columns: table => new
                {
                    ConsultantPaymentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConsultantId = table.Column<int>(type: "int", nullable: false),
                    StartDatePeriod = table.Column<DateTime>(type: "date", nullable: false),
                    EndDatePeriod = table.Column<DateTime>(type: "date", nullable: false),
                    ReferenceNumber = table.Column<int>(type: "int", nullable: false),
                    PaymentMethodId = table.Column<int>(type: "int", nullable: false),
                    PaymentAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserCreatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    LastUpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UserLastUpdatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CompanyId = table.Column<string>(type: "varchar(8)", maxLength: 8, nullable: false),
                    BankAccountId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CONSULTANT_PAYMENTS", x => x.ConsultantPaymentId);
                    table.ForeignKey(
                        name: "FK_CONSULTANT_PAYMENTS_BANK_ACCOUNTS_BankAccountId",
                        column: x => x.BankAccountId,
                        principalTable: "BANK_ACCOUNTS",
                        principalColumn: "BankAccountId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CONSULTANT_PAYMENTS_CONSULTANT_DETAILS_ConsultantId",
                        column: x => x.ConsultantId,
                        principalTable: "CONSULTANT_DETAILS",
                        principalColumn: "ConsultantId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CONSULTANT_PAYMENTS_PAYMENT_METHODS_PaymentMethodId",
                        column: x => x.PaymentMethodId,
                        principalTable: "PAYMENT_METHODS",
                        principalColumn: "PaymentMethodId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CONSULTANT_PAYMENTS_Users_UserCreatedBy",
                        column: x => x.UserCreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CONSULTANT_PAYMENTS_Users_UserLastUpdatedBy",
                        column: x => x.UserLastUpdatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BANK_ACCOUNTS_AccountingAccountId",
                table: "BANK_ACCOUNTS",
                column: "AccountingAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_BANK_ACCOUNTS_CostCenterId",
                table: "BANK_ACCOUNTS",
                column: "CostCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANT_PAYMENTS_BankAccountId",
                table: "CONSULTANT_PAYMENTS",
                column: "BankAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANT_PAYMENTS_ConsultantId",
                table: "CONSULTANT_PAYMENTS",
                column: "ConsultantId");

            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANT_PAYMENTS_ConsultantId_StartDatePeriod_EndDatePeriod",
                table: "CONSULTANT_PAYMENTS",
                columns: new[] { "ConsultantId", "StartDatePeriod", "EndDatePeriod" });

            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANT_PAYMENTS_PaymentMethodId",
                table: "CONSULTANT_PAYMENTS",
                column: "PaymentMethodId");

            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANT_PAYMENTS_UserCreatedBy",
                table: "CONSULTANT_PAYMENTS",
                column: "UserCreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANT_PAYMENTS_UserLastUpdatedBy",
                table: "CONSULTANT_PAYMENTS",
                column: "UserLastUpdatedBy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CONSULTANT_PAYMENTS");

            migrationBuilder.DropTable(
                name: "BANK_ACCOUNTS");
        }
    }
}
