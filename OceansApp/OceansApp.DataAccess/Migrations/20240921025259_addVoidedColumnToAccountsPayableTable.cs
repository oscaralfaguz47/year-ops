using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class addVoidedColumnToAccountsPayableTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Voided",
                table: "ACCOUNTS_PAYABLE",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_ACCOUNTS_PAYABLE_Voided",
                table: "ACCOUNTS_PAYABLE",
                column: "Voided");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ACCOUNTS_PAYABLE_Voided",
                table: "ACCOUNTS_PAYABLE");

            migrationBuilder.DropColumn(
                name: "Voided",
                table: "ACCOUNTS_PAYABLE");
        }
    }
}
