using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class updateTableNamesJournalPayablesAndAddPeriodDates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JOURNAL");

            migrationBuilder.DropTable(
                name: "JOURNAL_ENTRIES");

            migrationBuilder.CreateTable(
                name: "JOURNAL_ACCOUNTS_PAYABLE",
                columns: table => new
                {
                    JournalId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<string>(type: "varchar(8)", maxLength: 8, nullable: false),
                    StartDatePeriod = table.Column<DateTime>(type: "date", nullable: false),
                    EndDatePeriod = table.Column<DateTime>(type: "date", nullable: false),
                    TransactionStatusId = table.Column<int>(type: "int", nullable: false),
                    Entry = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false),
                    AccountingPackage = table.Column<string>(type: "varchar(4)", maxLength: 4, nullable: false),
                    EntryType = table.Column<string>(type: "varchar(4)", maxLength: 4, nullable: false),
                    AccountingDate = table.Column<DateTime>(type: "date", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserCreatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    LastUpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UserLastUpdatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JOURNAL_ACCOUNTS_PAYABLE", x => x.JournalId);
                    table.ForeignKey(
                        name: "FK_JOURNAL_ACCOUNTS_PAYABLE_COMPANIES_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "COMPANIES",
                        principalColumn: "CompanyId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JOURNAL_ACCOUNTS_PAYABLE_TRANSACTION_STATUSES_TransactionStatusId",
                        column: x => x.TransactionStatusId,
                        principalTable: "TRANSACTION_STATUSES",
                        principalColumn: "TransactionStatusId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JOURNAL_ACCOUNTS_PAYABLE_Users_UserCreatedBy",
                        column: x => x.UserCreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JOURNAL_ACCOUNTS_PAYABLE_Users_UserLastUpdatedBy",
                        column: x => x.UserLastUpdatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "JOURNAL_ACCOUNTS_PAYABLE_ENTRIES",
                columns: table => new
                {
                    JournalEntryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CostCenterId = table.Column<int>(type: "int", nullable: false),
                    AccountingAccountId = table.Column<int>(type: "int", nullable: false),
                    Reference = table.Column<string>(type: "varchar(249)", maxLength: 249, nullable: false),
                    Debit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Credit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AccountPayableId = table.Column<int>(type: "int", nullable: false),
                    JournalId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JOURNAL_ACCOUNTS_PAYABLE_ENTRIES", x => x.JournalEntryId);
                    table.ForeignKey(
                        name: "FK_JOURNAL_ACCOUNTS_PAYABLE_ENTRIES_ACCOUNTING_ACCOUNT_AccountingAccountId",
                        column: x => x.AccountingAccountId,
                        principalTable: "ACCOUNTING_ACCOUNT",
                        principalColumn: "AccountingAccountId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JOURNAL_ACCOUNTS_PAYABLE_ENTRIES_ACCOUNTS_PAYABLE_AccountPayableId",
                        column: x => x.AccountPayableId,
                        principalTable: "ACCOUNTS_PAYABLE",
                        principalColumn: "AccountPayableId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JOURNAL_ACCOUNTS_PAYABLE_ENTRIES_COST_CENTER_CostCenterId",
                        column: x => x.CostCenterId,
                        principalTable: "COST_CENTER",
                        principalColumn: "CostCenterId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JOURNAL_ACCOUNTS_PAYABLE_ENTRIES_JOURNAL_ACCOUNTS_PAYABLE_JournalId",
                        column: x => x.JournalId,
                        principalTable: "JOURNAL_ACCOUNTS_PAYABLE",
                        principalColumn: "JournalId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JOURNAL_ACCOUNTS_PAYABLE_AccountingDate",
                table: "JOURNAL_ACCOUNTS_PAYABLE",
                column: "AccountingDate");

            migrationBuilder.CreateIndex(
                name: "IX_JOURNAL_ACCOUNTS_PAYABLE_AccountingPackage",
                table: "JOURNAL_ACCOUNTS_PAYABLE",
                column: "AccountingPackage");

            migrationBuilder.CreateIndex(
                name: "IX_JOURNAL_ACCOUNTS_PAYABLE_CompanyId",
                table: "JOURNAL_ACCOUNTS_PAYABLE",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_JOURNAL_ACCOUNTS_PAYABLE_Entry",
                table: "JOURNAL_ACCOUNTS_PAYABLE",
                column: "Entry");

            migrationBuilder.CreateIndex(
                name: "IX_JOURNAL_ACCOUNTS_PAYABLE_TransactionStatusId",
                table: "JOURNAL_ACCOUNTS_PAYABLE",
                column: "TransactionStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_JOURNAL_ACCOUNTS_PAYABLE_UserCreatedBy",
                table: "JOURNAL_ACCOUNTS_PAYABLE",
                column: "UserCreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_JOURNAL_ACCOUNTS_PAYABLE_UserLastUpdatedBy",
                table: "JOURNAL_ACCOUNTS_PAYABLE",
                column: "UserLastUpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_JOURNAL_ACCOUNTS_PAYABLE_ENTRIES_AccountingAccountId",
                table: "JOURNAL_ACCOUNTS_PAYABLE_ENTRIES",
                column: "AccountingAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_JOURNAL_ACCOUNTS_PAYABLE_ENTRIES_AccountPayableId",
                table: "JOURNAL_ACCOUNTS_PAYABLE_ENTRIES",
                column: "AccountPayableId");

            migrationBuilder.CreateIndex(
                name: "IX_JOURNAL_ACCOUNTS_PAYABLE_ENTRIES_CostCenterId",
                table: "JOURNAL_ACCOUNTS_PAYABLE_ENTRIES",
                column: "CostCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_JOURNAL_ACCOUNTS_PAYABLE_ENTRIES_Credit",
                table: "JOURNAL_ACCOUNTS_PAYABLE_ENTRIES",
                column: "Credit");

            migrationBuilder.CreateIndex(
                name: "IX_JOURNAL_ACCOUNTS_PAYABLE_ENTRIES_Debit",
                table: "JOURNAL_ACCOUNTS_PAYABLE_ENTRIES",
                column: "Debit");

            migrationBuilder.CreateIndex(
                name: "IX_JOURNAL_ACCOUNTS_PAYABLE_ENTRIES_JournalId",
                table: "JOURNAL_ACCOUNTS_PAYABLE_ENTRIES",
                column: "JournalId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JOURNAL_ACCOUNTS_PAYABLE_ENTRIES");

            migrationBuilder.DropTable(
                name: "JOURNAL_ACCOUNTS_PAYABLE");

            migrationBuilder.CreateTable(
                name: "JOURNAL",
                columns: table => new
                {
                    JournalId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<string>(type: "varchar(8)", maxLength: 8, nullable: false),
                    TransactionStatusId = table.Column<int>(type: "int", nullable: false),
                    UserCreatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    UserLastUpdatedBy = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    AccountingDate = table.Column<DateTime>(type: "date", nullable: false),
                    AccountingPackage = table.Column<string>(type: "varchar(4)", maxLength: 4, nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Entry = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false),
                    EntryType = table.Column<string>(type: "varchar(4)", maxLength: 4, nullable: false),
                    LastUpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JOURNAL", x => x.JournalId);
                    table.ForeignKey(
                        name: "FK_JOURNAL_COMPANIES_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "COMPANIES",
                        principalColumn: "CompanyId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JOURNAL_TRANSACTION_STATUSES_TransactionStatusId",
                        column: x => x.TransactionStatusId,
                        principalTable: "TRANSACTION_STATUSES",
                        principalColumn: "TransactionStatusId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JOURNAL_Users_UserCreatedBy",
                        column: x => x.UserCreatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JOURNAL_Users_UserLastUpdatedBy",
                        column: x => x.UserLastUpdatedBy,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "JOURNAL_ENTRIES",
                columns: table => new
                {
                    JournalEntryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountingAccountId = table.Column<int>(type: "int", nullable: false),
                    AccountPayableId = table.Column<int>(type: "int", nullable: false),
                    CostCenterId = table.Column<int>(type: "int", nullable: false),
                    Credit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Debit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Reference = table.Column<string>(type: "varchar(249)", maxLength: 249, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JOURNAL_ENTRIES", x => x.JournalEntryId);
                    table.ForeignKey(
                        name: "FK_JOURNAL_ENTRIES_ACCOUNTING_ACCOUNT_AccountingAccountId",
                        column: x => x.AccountingAccountId,
                        principalTable: "ACCOUNTING_ACCOUNT",
                        principalColumn: "AccountingAccountId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JOURNAL_ENTRIES_ACCOUNTS_PAYABLE_AccountPayableId",
                        column: x => x.AccountPayableId,
                        principalTable: "ACCOUNTS_PAYABLE",
                        principalColumn: "AccountPayableId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_JOURNAL_ENTRIES_COST_CENTER_CostCenterId",
                        column: x => x.CostCenterId,
                        principalTable: "COST_CENTER",
                        principalColumn: "CostCenterId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JOURNAL_AccountingDate",
                table: "JOURNAL",
                column: "AccountingDate");

            migrationBuilder.CreateIndex(
                name: "IX_JOURNAL_AccountingPackage",
                table: "JOURNAL",
                column: "AccountingPackage");

            migrationBuilder.CreateIndex(
                name: "IX_JOURNAL_CompanyId",
                table: "JOURNAL",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_JOURNAL_Entry",
                table: "JOURNAL",
                column: "Entry");

            migrationBuilder.CreateIndex(
                name: "IX_JOURNAL_TransactionStatusId",
                table: "JOURNAL",
                column: "TransactionStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_JOURNAL_UserCreatedBy",
                table: "JOURNAL",
                column: "UserCreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_JOURNAL_UserLastUpdatedBy",
                table: "JOURNAL",
                column: "UserLastUpdatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_JOURNAL_ENTRIES_AccountingAccountId",
                table: "JOURNAL_ENTRIES",
                column: "AccountingAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_JOURNAL_ENTRIES_AccountPayableId",
                table: "JOURNAL_ENTRIES",
                column: "AccountPayableId");

            migrationBuilder.CreateIndex(
                name: "IX_JOURNAL_ENTRIES_CostCenterId",
                table: "JOURNAL_ENTRIES",
                column: "CostCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_JOURNAL_ENTRIES_Credit",
                table: "JOURNAL_ENTRIES",
                column: "Credit");

            migrationBuilder.CreateIndex(
                name: "IX_JOURNAL_ENTRIES_Debit",
                table: "JOURNAL_ENTRIES",
                column: "Debit");
        }
    }
}
