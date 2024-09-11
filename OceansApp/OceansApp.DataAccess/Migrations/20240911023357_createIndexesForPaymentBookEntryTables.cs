using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class createIndexesForPaymentBookEntryTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_PAYMENT_BOOK_ENTRIES_PARENT_CompanyId_TransactionStatusId_ParentId",
                table: "PAYMENT_BOOK_ENTRIES_PARENT",
                columns: new[] { "CompanyId", "TransactionStatusId", "ParentId" });

            migrationBuilder.CreateIndex(
                name: "IX_PAYMENT_BOOK_ENTRIES_CHILD_Voided",
                table: "PAYMENT_BOOK_ENTRIES_CHILD",
                column: "Voided");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PAYMENT_BOOK_ENTRIES_PARENT_CompanyId_TransactionStatusId_ParentId",
                table: "PAYMENT_BOOK_ENTRIES_PARENT");

            migrationBuilder.DropIndex(
                name: "IX_PAYMENT_BOOK_ENTRIES_CHILD_Voided",
                table: "PAYMENT_BOOK_ENTRIES_CHILD");
        }
    }
}
