using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class createIndexForAccountsPaybleTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ACCOUNTS_PAYABLE_StartDatePeriod_EndDatePeriod",
                table: "ACCOUNTS_PAYABLE",
                columns: new[] { "StartDatePeriod", "EndDatePeriod" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ACCOUNTS_PAYABLE_StartDatePeriod_EndDatePeriod",
                table: "ACCOUNTS_PAYABLE");
        }
    }
}
