using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class updateAccountsPayableAndJournalEntriesTablesCreateGlobalConsecutives : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ACCOUNTS_PAYABLE_JOURNAL_JournalId",
                table: "ACCOUNTS_PAYABLE");

            migrationBuilder.DropIndex(
                name: "IX_ACCOUNTS_PAYABLE_JournalId",
                table: "ACCOUNTS_PAYABLE");

            migrationBuilder.DropColumn(
                name: "JournalId",
                table: "ACCOUNTS_PAYABLE");

            migrationBuilder.AddColumn<int>(
                name: "AccountPayableId",
                table: "JOURNAL_ENTRIES",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "GLOBAL_CONSECUTIVES",
                columns: table => new
                {
                    GlobalConsecutiveId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false),
                    ConsecutiveNumber = table.Column<int>(type: "int", nullable: false),
                    CompanyId = table.Column<string>(type: "varchar(8)", maxLength: 8, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GLOBAL_CONSECUTIVES", x => x.GlobalConsecutiveId);
                    table.ForeignKey(
                        name: "FK_GLOBAL_CONSECUTIVES_COMPANIES_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "COMPANIES",
                        principalColumn: "CompanyId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JOURNAL_ENTRIES_AccountPayableId",
                table: "JOURNAL_ENTRIES",
                column: "AccountPayableId");

            migrationBuilder.CreateIndex(
                name: "IX_GLOBAL_CONSECUTIVES_CompanyId",
                table: "GLOBAL_CONSECUTIVES",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_GLOBAL_CONSECUTIVES_GlobalConsecutiveId",
                table: "GLOBAL_CONSECUTIVES",
                column: "GlobalConsecutiveId");

            migrationBuilder.CreateIndex(
                name: "IX_GLOBAL_CONSECUTIVES_Name",
                table: "GLOBAL_CONSECUTIVES",
                column: "Name");

            migrationBuilder.AddForeignKey(
                name: "FK_JOURNAL_ENTRIES_ACCOUNTS_PAYABLE_AccountPayableId",
                table: "JOURNAL_ENTRIES",
                column: "AccountPayableId",
                principalTable: "ACCOUNTS_PAYABLE",
                principalColumn: "AccountPayableId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JOURNAL_ENTRIES_ACCOUNTS_PAYABLE_AccountPayableId",
                table: "JOURNAL_ENTRIES");

            migrationBuilder.DropTable(
                name: "GLOBAL_CONSECUTIVES");

            migrationBuilder.DropIndex(
                name: "IX_JOURNAL_ENTRIES_AccountPayableId",
                table: "JOURNAL_ENTRIES");

            migrationBuilder.DropColumn(
                name: "AccountPayableId",
                table: "JOURNAL_ENTRIES");

            migrationBuilder.AddColumn<int>(
                name: "JournalId",
                table: "ACCOUNTS_PAYABLE",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ACCOUNTS_PAYABLE_JournalId",
                table: "ACCOUNTS_PAYABLE",
                column: "JournalId");

            migrationBuilder.AddForeignKey(
                name: "FK_ACCOUNTS_PAYABLE_JOURNAL_JournalId",
                table: "ACCOUNTS_PAYABLE",
                column: "JournalId",
                principalTable: "JOURNAL",
                principalColumn: "JournalId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
