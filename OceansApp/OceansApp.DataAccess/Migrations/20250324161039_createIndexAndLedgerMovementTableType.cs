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

            // Create the User-Defined Table Type
            migrationBuilder.Sql(@"
        CREATE TYPE LedgerMovementType AS TABLE
        (
            IdSeat NVARCHAR(10),
            Consecutive INT,
            Date DATETIME2(7),
            LocalDebit DECIMAL(18,2),
            LocalCredit DECIMAL(18,2),
            AccountingType NVARCHAR(1),
            RecordDate DATETIME2(7),
            AccountingAccountId INT,
            CompanyId NVARCHAR(8),
            CostCenterId INT
        );
    ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IX_LEDGER_MOVEMENT_UniqueRecord ON LEDGER_MOVEMENT;");

            migrationBuilder.Sql("DROP TYPE LedgerMovementType;");
        }

    }
}
