using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class createJounalAndJournalEntriesTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "JournalId",
                table: "ACCOUNTS_PAYABLE",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "JOURNAL",
                columns: table => new
                {
                    JournalId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<string>(type: "varchar(8)", maxLength: 8, nullable: false),
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
                    table.PrimaryKey("PK_JOURNAL", x => x.JournalId);
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
                    CostCenterId = table.Column<int>(type: "int", nullable: false),
                    AccountingAccountId = table.Column<int>(type: "int", nullable: false),
                    Reference = table.Column<string>(type: "varchar(249)", maxLength: 249, nullable: false),
                    Debit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Credit = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
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
                        name: "FK_JOURNAL_ENTRIES_COST_CENTER_CostCenterId",
                        column: x => x.CostCenterId,
                        principalTable: "COST_CENTER",
                        principalColumn: "CostCenterId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ACCOUNTS_PAYABLE_JournalId",
                table: "ACCOUNTS_PAYABLE",
                column: "JournalId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_ACCOUNTS_PAYABLE_JOURNAL_JournalId",
                table: "ACCOUNTS_PAYABLE",
                column: "JournalId",
                principalTable: "JOURNAL",
                principalColumn: "JournalId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ACCOUNTS_PAYABLE_JOURNAL_JournalId",
                table: "ACCOUNTS_PAYABLE");

            migrationBuilder.DropTable(
                name: "JOURNAL");

            migrationBuilder.DropTable(
                name: "JOURNAL_ENTRIES");

            migrationBuilder.DropIndex(
                name: "IX_ACCOUNTS_PAYABLE_JournalId",
                table: "ACCOUNTS_PAYABLE");

            migrationBuilder.DropColumn(
                name: "JournalId",
                table: "ACCOUNTS_PAYABLE");
        }
    }
}
