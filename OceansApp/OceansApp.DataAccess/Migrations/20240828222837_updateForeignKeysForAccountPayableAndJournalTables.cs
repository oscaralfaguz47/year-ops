using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class updateForeignKeysForAccountPayableAndJournalTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ACCOUNTS_PAYABLE_CompanyId",
                table: "ACCOUNTS_PAYABLE",
                column: "CompanyId");

            migrationBuilder.AddForeignKey(
                name: "FK_ACCOUNTS_PAYABLE_COMPANIES_CompanyId",
                table: "ACCOUNTS_PAYABLE",
                column: "CompanyId",
                principalTable: "COMPANIES",
                principalColumn: "CompanyId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_JOURNAL_COMPANIES_CompanyId",
                table: "JOURNAL",
                column: "CompanyId",
                principalTable: "COMPANIES",
                principalColumn: "CompanyId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ACCOUNTS_PAYABLE_COMPANIES_CompanyId",
                table: "ACCOUNTS_PAYABLE");

            migrationBuilder.DropForeignKey(
                name: "FK_JOURNAL_COMPANIES_CompanyId",
                table: "JOURNAL");

            migrationBuilder.DropIndex(
                name: "IX_ACCOUNTS_PAYABLE_CompanyId",
                table: "ACCOUNTS_PAYABLE");
        }
    }
}
