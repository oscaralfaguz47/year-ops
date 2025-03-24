using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class CreateSPInsertLedgerMovements : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
        CREATE PROCEDURE SP_LEDGER_MOVEMENT_InsertLedgerMovements
            @LedgerMovements LedgerMovementType READONLY,
            @InsertedCount INT OUTPUT
        AS
        BEGIN
            SET NOCOUNT ON;

            INSERT INTO LEDGER_MOVEMENT (
                IdSeat,
                Consecutive,
                Date,
                LocalDebit,
                LocalCredit,
                AccountingType,
                RecordDate,
                AccountingAccountId,
                CompanyId,
                CostCenterId
            )
            SELECT 
                lm.IdSeat,
                lm.Consecutive,
                lm.Date,
                lm.LocalDebit,
                lm.LocalCredit,
                lm.AccountingType,
                lm.RecordDate,
                lm.AccountingAccountId,
                lm.CompanyId,
                lm.CostCenterId
            FROM @LedgerMovements lm
            WHERE NOT EXISTS (
                SELECT 1 
                FROM LEDGER_MOVEMENT existing
                WHERE existing.IdSeat = lm.IdSeat
                  AND existing.CostCenterId = lm.CostCenterId
                  AND existing.AccountingAccountId = lm.AccountingAccountId
                  AND existing.LocalDebit = lm.LocalDebit
                  AND existing.LocalCredit = lm.LocalCredit
                  AND existing.Consecutive = lm.Consecutive
                  AND existing.CompanyId = lm.CompanyId
            );

            SET @InsertedCount = @@ROWCOUNT;
        END
    ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE SP_LEDGER_MOVEMENT_InsertLedgerMovements;");
        }

    }
}
