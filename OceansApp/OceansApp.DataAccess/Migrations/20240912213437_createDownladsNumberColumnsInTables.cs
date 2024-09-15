using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class createDownladsNumberColumnsInTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DownloadsNumber",
                table: "PAYMENT_BOOK_ENTRIES_PARENT",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DownloadsNumber",
                table: "JOURNAL_ACCOUNTS_PAYABLE",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DownloadsNumber",
                table: "PAYMENT_BOOK_ENTRIES_PARENT");

            migrationBuilder.DropColumn(
                name: "DownloadsNumber",
                table: "JOURNAL_ACCOUNTS_PAYABLE");
        }
    }
}
