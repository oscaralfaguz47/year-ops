using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class oneUpdateSPInsertLedgerMovements : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //UPDATE STORED PROCEDURE
            var sp = @"CREATE PROCEDURE SP_LEDGER_MOVEMENT_InsertLedgerMovements
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
                    aa.AccountingAccountId,
                    lm.CompanyId,
                    cc.CostCenterId
                FROM @LedgerMovements lm
                INNER JOIN ACCOUNTING_ACCOUNT aa 
                    ON aa.AccountingAccountCode = lm.AccountingAccountCode
                   AND aa.CompanyId = lm.CompanyId
                INNER JOIN COST_CENTER cc 
                    ON cc.CostCenterCode = lm.CostCenterCode
                   AND cc.CompanyId = lm.CompanyId
                WHERE NOT EXISTS (
                    SELECT 1 
                    FROM LEDGER_MOVEMENT existing
                    WHERE existing.IdSeat = lm.IdSeat
                      AND existing.Consecutive = lm.Consecutive
                      AND existing.LocalDebit = lm.LocalDebit
                      AND existing.LocalCredit = lm.LocalCredit
                      AND existing.CompanyId = lm.CompanyId
                      AND existing.AccountingAccountId = aa.AccountingAccountId
                      AND existing.CostCenterId = cc.CostCenterId
                );
            
                SET @InsertedCount = @@ROWCOUNT;
            END;";

            // Delete stored procedure if exists
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS SP_LEDGER_MOVEMENT_InsertLedgerMovements");

            // Create new stored Procedure
            migrationBuilder.Sql(sp);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // restart the stored procedure to the original version
            var spOriginal = @"CREATE PROCEDURE SP_LEDGER_MOVEMENT_InsertLedgerMovements
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
        END;";

            migrationBuilder.Sql("DROP PROCEDURE IF SP_LEDGER_MOVEMENT_InsertLedgerMovements");
            migrationBuilder.Sql(spOriginal);
        }
    }
}
