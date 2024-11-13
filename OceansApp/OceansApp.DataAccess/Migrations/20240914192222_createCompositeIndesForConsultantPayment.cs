using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class createCompositeIndesForConsultantPayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CONSULTANT_PAYMENTS_ReferenceNumber",
                table: "CONSULTANT_PAYMENTS");

            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANT_PAYMENTS_ReferenceNumber_BankAccountId",
                table: "CONSULTANT_PAYMENTS",
                columns: new[] { "ReferenceNumber", "BankAccountId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CONSULTANT_PAYMENTS_ReferenceNumber_BankAccountId",
                table: "CONSULTANT_PAYMENTS");

            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANT_PAYMENTS_ReferenceNumber",
                table: "CONSULTANT_PAYMENTS",
                column: "ReferenceNumber",
                unique: true);
        }
    }
}
