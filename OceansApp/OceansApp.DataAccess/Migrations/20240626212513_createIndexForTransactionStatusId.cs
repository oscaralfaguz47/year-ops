using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class createIndexForTransactionStatusId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_TRANSACTION_STATUSES_TransactionStatusId",
                table: "TRANSACTION_STATUSES",
                column: "TransactionStatusId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TRANSACTION_STATUSES_TransactionStatusId",
                table: "TRANSACTION_STATUSES");
        }
    }
}
