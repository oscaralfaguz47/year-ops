using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    public partial class addTransactionStatusToInterviewsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TransactionStatusId",
                table: "INTERVIEWS",
                type: "int",
                nullable: false);

            migrationBuilder.CreateIndex(
                name: "IX_INTERVIEWS_TransactionStatusId",
                table: "INTERVIEWS",
                column: "TransactionStatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_INTERVIEWS_TRANSACTION_STATUSES_TransactionStatusId",
                table: "INTERVIEWS",
                column: "TransactionStatusId",
                principalTable: "TRANSACTION_STATUSES",
                principalColumn: "TransactionStatusId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_INTERVIEWS_TRANSACTION_STATUSES_TransactionStatusId",
                table: "INTERVIEWS");

            migrationBuilder.DropIndex(
                name: "IX_INTERVIEWS_TransactionStatusId",
                table: "INTERVIEWS");

            migrationBuilder.DropColumn(
                name: "TransactionStatusId",
                table: "INTERVIEWS");
        }
    }
}
