using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class createIndexAndLedgerMovementTableType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create unique index
            migrationBuilder.Sql(@"
        CREATE UNIQUE INDEX IX_LEDGER_MOVEMENT_UniqueRecord
        ON LEDGER_MOVEMENT (
            IdSeat,
            CostCenterId,
            AccountingAccountId,
            LocalDebit,
            LocalCredit,
            Consecutive,
            CompanyId
        );
    ");

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IX_LEDGER_MOVEMENT_UniqueRecord ON LEDGER_MOVEMENT;");
        }

    }
}
