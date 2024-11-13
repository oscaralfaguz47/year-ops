using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class createAccountsPayableTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ACCOUNTS_PAYABLE",
                columns: table => new
                {
                    AccountPayableId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ConsultantId = table.Column<int>(type: "int", nullable: false),
                    StartDatePeriod = table.Column<DateTime>(type: "date", nullable: false),
                    EndDatePeriod = table.Column<DateTime>(type: "date", nullable: false),
                    AccountingDate = table.Column<DateTime>(type: "date", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BalanceAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserCreatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    LastUpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UserLastUpdatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CompanyId = table.Column<string>(type: "varchar(8)", maxLength: 8, nullable: false),
                    TransactionStatusId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ACCOUNTS_PAYABLE", x => x.AccountPayableId);
                    table.ForeignKey(
                        name: "FK_ACCOUNTS_PAYABLE_CONSULTANT_DETAILS_ConsultantId",
                        column: x => x.ConsultantId,
                        principalTable: "CONSULTANT_DETAILS",
                        principalColumn: "ConsultantId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ACCOUNTS_PAYABLE_TRANSACTION_STATUSES_TransactionStatusId",
                        column: x => x.TransactionStatusId,
                        principalTable: "TRANSACTION_STATUSES",
                        principalColumn: "TransactionStatusId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ACCOUNTS_PAYABLE_Users_UserCreatedBy",
                        column: x => x.UserCreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ACCOUNTS_PAYABLE_Users_UserLastUpdatedBy",
                        column: x => x.UserLastUpdatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ACCOUNTS_PAYABLE_AccountingDate",
                table: "ACCOUNTS_PAYABLE",
                column: "AccountingDate");

            migrationBuilder.CreateIndex(
                name: "IX_ACCOUNTS_PAYABLE_BalanceAmount",
                table: "ACCOUNTS_PAYABLE",
                column: "BalanceAmount");

            migrationBuilder.CreateIndex(
                name: "IX_ACCOUNTS_PAYABLE_ConsultantId",
                table: "ACCOUNTS_PAYABLE",
                column: "ConsultantId");

            migrationBuilder.CreateIndex(
                name: "IX_ACCOUNTS_PAYABLE_ConsultantId_StartDatePeriod_EndDatePeriod",
                table: "ACCOUNTS_PAYABLE",
                columns: new[] { "ConsultantId", "StartDatePeriod", "EndDatePeriod" });

            migrationBuilder.CreateIndex(
                name: "IX_ACCOUNTS_PAYABLE_TransactionStatusId",
                table: "ACCOUNTS_PAYABLE",
                column: "TransactionStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_ACCOUNTS_PAYABLE_UserCreatedBy",
                table: "ACCOUNTS_PAYABLE",
                column: "UserCreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ACCOUNTS_PAYABLE_UserLastUpdatedBy",
                table: "ACCOUNTS_PAYABLE",
                column: "UserLastUpdatedBy");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ACCOUNTS_PAYABLE");
        }
    }
}
