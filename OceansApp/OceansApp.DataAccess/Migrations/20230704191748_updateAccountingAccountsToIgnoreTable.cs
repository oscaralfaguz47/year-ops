using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    public partial class updateAccountingAccountsToIgnoreTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_CALCULATOR_ACCOUNTING_ACCOUNTS_TO_IGNORE",
                table: "CALCULATOR_ACCOUNTING_ACCOUNTS_TO_IGNORE");

            migrationBuilder.DropColumn(
                name: "IdAccountingAccount",
                table: "CALCULATOR_ACCOUNTING_ACCOUNTS_TO_IGNORE");

            migrationBuilder.AddColumn<int>(
                name: "AccountingAccountId",
                table: "CALCULATOR_ACCOUNTING_ACCOUNTS_TO_IGNORE",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreationDate",
                table: "CALCULATOR_ACCOUNTING_ACCOUNTS_TO_IGNORE",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddPrimaryKey(
                name: "PK_CALCULATOR_ACCOUNTING_ACCOUNTS_TO_IGNORE",
                table: "CALCULATOR_ACCOUNTING_ACCOUNTS_TO_IGNORE",
                column: "AccountingAccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_CALCULATOR_ACCOUNTING_ACCOUNTS_TO_IGNORE_ACCOUNTING_ACCOUNT_AccountingAccountId",
                table: "CALCULATOR_ACCOUNTING_ACCOUNTS_TO_IGNORE",
                column: "AccountingAccountId",
                principalTable: "ACCOUNTING_ACCOUNT",
                principalColumn: "AccountingAccountId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CALCULATOR_ACCOUNTING_ACCOUNTS_TO_IGNORE_ACCOUNTING_ACCOUNT_AccountingAccountId",
                table: "CALCULATOR_ACCOUNTING_ACCOUNTS_TO_IGNORE");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CALCULATOR_ACCOUNTING_ACCOUNTS_TO_IGNORE",
                table: "CALCULATOR_ACCOUNTING_ACCOUNTS_TO_IGNORE");

            migrationBuilder.DropColumn(
                name: "AccountingAccountId",
                table: "CALCULATOR_ACCOUNTING_ACCOUNTS_TO_IGNORE");

            migrationBuilder.DropColumn(
                name: "CreationDate",
                table: "CALCULATOR_ACCOUNTING_ACCOUNTS_TO_IGNORE");

            migrationBuilder.AddColumn<string>(
                name: "IdAccountingAccount",
                table: "CALCULATOR_ACCOUNTING_ACCOUNTS_TO_IGNORE",
                type: "nvarchar(25)",
                maxLength: 25,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CALCULATOR_ACCOUNTING_ACCOUNTS_TO_IGNORE",
                table: "CALCULATOR_ACCOUNTING_ACCOUNTS_TO_IGNORE",
                column: "IdAccountingAccount");
        }
    }
}
