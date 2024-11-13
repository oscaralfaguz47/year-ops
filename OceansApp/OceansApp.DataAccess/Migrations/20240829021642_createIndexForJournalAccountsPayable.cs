using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class createIndexForJournalAccountsPayable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_JOURNAL_ACCOUNTS_PAYABLE_EndDatePeriod",
                table: "JOURNAL_ACCOUNTS_PAYABLE",
                column: "EndDatePeriod");

            migrationBuilder.CreateIndex(
                name: "IX_JOURNAL_ACCOUNTS_PAYABLE_StartDatePeriod",
                table: "JOURNAL_ACCOUNTS_PAYABLE",
                column: "StartDatePeriod");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_JOURNAL_ACCOUNTS_PAYABLE_EndDatePeriod",
                table: "JOURNAL_ACCOUNTS_PAYABLE");

            migrationBuilder.DropIndex(
                name: "IX_JOURNAL_ACCOUNTS_PAYABLE_StartDatePeriod",
                table: "JOURNAL_ACCOUNTS_PAYABLE");
        }
    }
}
