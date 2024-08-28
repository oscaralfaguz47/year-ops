using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class addPaymentMethodAndBankAccountsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PAYMENT_METHOD_AND_BANK_ACCOUNTS",
                columns: table => new
                {
                    PaymentMethodId = table.Column<int>(type: "int", nullable: false),
                    BankAccountId = table.Column<int>(type: "int", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PAYMENT_METHOD_AND_BANK_ACCOUNTS", x => new { x.PaymentMethodId, x.BankAccountId });
                    table.ForeignKey(
                        name: "FK_PAYMENT_METHOD_AND_BANK_ACCOUNTS_BANK_ACCOUNTS_BankAccountId",
                        column: x => x.BankAccountId,
                        principalTable: "BANK_ACCOUNTS",
                        principalColumn: "BankAccountId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PAYMENT_METHOD_AND_BANK_ACCOUNTS_PAYMENT_METHODS_PaymentMethodId",
                        column: x => x.PaymentMethodId,
                        principalTable: "PAYMENT_METHODS",
                        principalColumn: "PaymentMethodId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PAYMENT_METHOD_AND_BANK_ACCOUNTS_BankAccountId",
                table: "PAYMENT_METHOD_AND_BANK_ACCOUNTS",
                column: "BankAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_PAYMENT_METHOD_AND_BANK_ACCOUNTS_PaymentMethodId",
                table: "PAYMENT_METHOD_AND_BANK_ACCOUNTS",
                column: "PaymentMethodId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PAYMENT_METHOD_AND_BANK_ACCOUNTS");
        }
    }
}
