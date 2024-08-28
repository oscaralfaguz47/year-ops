using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class twoUpdateConsultantPaymentTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AccountPayableId",
                table: "CONSULTANT_PAYMENTS",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_CONSULTANT_PAYMENTS_AccountPayableId",
                table: "CONSULTANT_PAYMENTS",
                column: "AccountPayableId");

            migrationBuilder.AddForeignKey(
                name: "FK_CONSULTANT_PAYMENTS_ACCOUNTS_PAYABLE_AccountPayableId",
                table: "CONSULTANT_PAYMENTS",
                column: "AccountPayableId",
                principalTable: "ACCOUNTS_PAYABLE",
                principalColumn: "AccountPayableId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CONSULTANT_PAYMENTS_ACCOUNTS_PAYABLE_AccountPayableId",
                table: "CONSULTANT_PAYMENTS");

            migrationBuilder.DropIndex(
                name: "IX_CONSULTANT_PAYMENTS_AccountPayableId",
                table: "CONSULTANT_PAYMENTS");

            migrationBuilder.DropColumn(
                name: "AccountPayableId",
                table: "CONSULTANT_PAYMENTS");
        }
    }
}
