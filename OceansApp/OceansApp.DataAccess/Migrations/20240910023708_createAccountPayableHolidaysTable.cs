using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class createAccountPayableHolidaysTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ACCOUNT_PAYABLE_HOLIDAYS",
                columns: table => new
                {
                    AccountPayableHolidayId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountPayableId = table.Column<int>(type: "int", nullable: false),
                    Reference = table.Column<string>(type: "nvarchar(249)", maxLength: 249, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ACCOUNT_PAYABLE_HOLIDAYS", x => x.AccountPayableHolidayId);
                    table.ForeignKey(
                        name: "FK_ACCOUNT_PAYABLE_HOLIDAYS_ACCOUNTS_PAYABLE_AccountPayableId",
                        column: x => x.AccountPayableId,
                        principalTable: "ACCOUNTS_PAYABLE",
                        principalColumn: "AccountPayableId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ACCOUNT_PAYABLE_HOLIDAYS_AccountPayableId",
                table: "ACCOUNT_PAYABLE_HOLIDAYS",
                column: "AccountPayableId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ACCOUNT_PAYABLE_HOLIDAYS");
        }
    }
}
