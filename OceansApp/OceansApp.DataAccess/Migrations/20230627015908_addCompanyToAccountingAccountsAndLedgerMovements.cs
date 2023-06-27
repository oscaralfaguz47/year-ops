using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    public partial class addCompanyToAccountingAccountsAndLedgerMovements : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LEDGER_MOVEMENT_ACCOUNTING_ACCOUNT_IdAccountingAccount",
                table: "LEDGER_MOVEMENT");

            migrationBuilder.DropIndex(
                name: "IX_LEDGER_MOVEMENT_IdAccountingAccount",
                table: "LEDGER_MOVEMENT");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ACCOUNTING_ACCOUNT",
                table: "ACCOUNTING_ACCOUNT");

            migrationBuilder.DropColumn(
                name: "IdAccountingAccount",
                table: "LEDGER_MOVEMENT");

            migrationBuilder.DropColumn(
                name: "IdAccountingAccount",
                table: "ACCOUNTING_ACCOUNT");

            migrationBuilder.AddColumn<int>(
                name: "AccountingAccountId",
                table: "LEDGER_MOVEMENT",
                type: "int",
                maxLength: 25,
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "CompanyId",
                table: "LEDGER_MOVEMENT",
                type: "nvarchar(8)",
                maxLength: 8,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AccountingAccountId",
                table: "ACCOUNTING_ACCOUNT",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<string>(
                name: "AccountingAccountCode",
                table: "ACCOUNTING_ACCOUNT",
                type: "nvarchar(25)",
                maxLength: 25,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanyId",
                table: "ACCOUNTING_ACCOUNT",
                type: "nvarchar(8)",
                maxLength: 8,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ACCOUNTING_ACCOUNT",
                table: "ACCOUNTING_ACCOUNT",
                column: "AccountingAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_LEDGER_MOVEMENT_AccountingAccountId",
                table: "LEDGER_MOVEMENT",
                column: "AccountingAccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_LEDGER_MOVEMENT_ACCOUNTING_ACCOUNT_AccountingAccountId",
                table: "LEDGER_MOVEMENT",
                column: "AccountingAccountId",
                principalTable: "ACCOUNTING_ACCOUNT",
                principalColumn: "AccountingAccountId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LEDGER_MOVEMENT_ACCOUNTING_ACCOUNT_AccountingAccountId",
                table: "LEDGER_MOVEMENT");

            migrationBuilder.DropIndex(
                name: "IX_LEDGER_MOVEMENT_AccountingAccountId",
                table: "LEDGER_MOVEMENT");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ACCOUNTING_ACCOUNT",
                table: "ACCOUNTING_ACCOUNT");

            migrationBuilder.DropColumn(
                name: "AccountingAccountId",
                table: "LEDGER_MOVEMENT");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "LEDGER_MOVEMENT");

            migrationBuilder.DropColumn(
                name: "AccountingAccountId",
                table: "ACCOUNTING_ACCOUNT");

            migrationBuilder.DropColumn(
                name: "AccountingAccountCode",
                table: "ACCOUNTING_ACCOUNT");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "ACCOUNTING_ACCOUNT");

            migrationBuilder.AddColumn<string>(
                name: "IdAccountingAccount",
                table: "LEDGER_MOVEMENT",
                type: "nvarchar(25)",
                maxLength: 25,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "IdAccountingAccount",
                table: "ACCOUNTING_ACCOUNT",
                type: "nvarchar(25)",
                maxLength: 25,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ACCOUNTING_ACCOUNT",
                table: "ACCOUNTING_ACCOUNT",
                column: "IdAccountingAccount");

            migrationBuilder.CreateIndex(
                name: "IX_LEDGER_MOVEMENT_IdAccountingAccount",
                table: "LEDGER_MOVEMENT",
                column: "IdAccountingAccount");

            migrationBuilder.AddForeignKey(
                name: "FK_LEDGER_MOVEMENT_ACCOUNTING_ACCOUNT_IdAccountingAccount",
                table: "LEDGER_MOVEMENT",
                column: "IdAccountingAccount",
                principalTable: "ACCOUNTING_ACCOUNT",
                principalColumn: "IdAccountingAccount",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
