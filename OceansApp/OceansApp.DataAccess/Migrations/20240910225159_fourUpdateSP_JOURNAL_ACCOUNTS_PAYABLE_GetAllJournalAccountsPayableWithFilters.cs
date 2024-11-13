using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class fourUpdateSP_JOURNAL_ACCOUNTS_PAYABLE_GetAllJournalAccountsPayableWithFilters : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //UPDATE STORED PROCEDURE
            var sp = @"CREATE PROCEDURE SP_JOURNAL_ACCOUNTS_PAYABLE_GetAllJournalAccountsPayableWithFilters
            @CompanyId VARCHAR(8),
            @TransactionStatusId INT,
            @FieldToOrder VARCHAR(255),
            @DirectionOrder VARCHAR(255),
            @Skip INT,
            @Take INT,
            @TotalCount INT OUTPUT
            AS
            BEGIN
                SELECT 
                JAP.Entry AS SeatNumber,
                JAP.AccountingDate,
                JAP.StartDatePeriod,
                JAP.EndDatePeriod,
                CO.Name AS CompanyName,
                TS.Name AS TransactionStatusName,
                COUNT(*) OVER() AS TotalCount  -- Calcula el número total de filas sin tener en cuenta la paginación
            FROM JOURNAL_ACCOUNTS_PAYABLE JAP
            INNER JOIN COMPANIES CO ON JAP.CompanyId = CO.CompanyId
            INNER JOIN TRANSACTION_STATUSES TS ON JAP.TransactionStatusId = TS.TransactionStatusId
            WHERE (@CompanyId IS NULL OR JAP.CompanyId = @CompanyId)
            AND (@TransactionStatusId IS NULL OR JAP.TransactionStatusId = @TransactionStatusId)
            ORDER BY 
                CASE WHEN @FieldToOrder = 'SeatNumber' AND @DirectionOrder = 'ASC' THEN JAP.Entry END ASC,
                CASE WHEN @FieldToOrder = 'SeatNumber' AND @DirectionOrder = 'DESC' THEN JAP.Entry END DESC,
                CASE WHEN @FieldToOrder = 'AccountingDate' AND @DirectionOrder = 'ASC' THEN JAP.AccountingDate END ASC,
                CASE WHEN @FieldToOrder = 'AccountingDate' AND @DirectionOrder = 'DESC' THEN JAP.AccountingDate END DESC,
                CASE WHEN @FieldToOrder = 'StartDatePeriod' AND @DirectionOrder = 'ASC' THEN JAP.StartDatePeriod END ASC,
                CASE WHEN @FieldToOrder = 'StartDatePeriod' AND @DirectionOrder = 'DESC' THEN JAP.StartDatePeriod END DESC,
                CASE WHEN @FieldToOrder = 'EndDatePeriod' AND @DirectionOrder = 'ASC' THEN JAP.EndDatePeriod END ASC,
                CASE WHEN @FieldToOrder = 'EndDatePeriod' AND @DirectionOrder = 'DESC' THEN JAP.EndDatePeriod END DESC,
                CASE WHEN @FieldToOrder = 'CompanyName' AND @DirectionOrder = 'ASC' THEN CO.Name END ASC,
                CASE WHEN @FieldToOrder = 'CompanyName' AND @DirectionOrder = 'DESC' THEN CO.Name END DESC,
                CASE WHEN @FieldToOrder = 'TransactionStatusName' AND @DirectionOrder = 'ASC' THEN TS.Name END ASC,
                CASE WHEN @FieldToOrder = 'TransactionStatusName' AND @DirectionOrder = 'DESC' THEN TS.Name END DESC,
                JAP.Entry
            OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;
            
            -- Asignamos el valor total de filas al parámetro de salida
            SET @TotalCount = (SELECT COUNT(*) FROM JOURNAL_ACCOUNTS_PAYABLE JAP2
                               WHERE (@CompanyId IS NULL OR JAP2.CompanyId = @CompanyId)
                               AND (@TransactionStatusId IS NULL OR JAP2.TransactionStatusId = @TransactionStatusId));
            END
            ";

            // Delete stored procedure if exists
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS SP_JOURNAL_ACCOUNTS_PAYABLE_GetAllJournalAccountsPayableWithFilters");

            // Create new stored Procedure
            migrationBuilder.Sql(sp);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // restart the stored procedure to the original version
            var spOriginal = @"CREATE PROCEDURE SP_JOURNAL_ACCOUNTS_PAYABLE_GetAllJournalAccountsPayableWithFilters
            @CompanyId VARCHAR(8),
            @TransactionStatusId INT,
            @FieldToOrder VARCHAR(255),
            @DirectionOrder VARCHAR(255),
            @Skip INT,
            @Take INT,
            @TotalCount INT OUTPUT
            AS
            BEGIN
                SELECT 
                JAP.Entry AS SeatNumber,
                JAP.AccountingDate,
                JAP.StartDatePeriod,
                JAP.EndDatePeriod,
                CO.Name AS CompanyName,
                TS.Name AS TransactionStatusName,
                COUNT(*) OVER() AS TotalCount  -- Calcula el número total de filas sin tener en cuenta la paginación
            FROM JOURNAL_ACCOUNTS_PAYABLE JAP
            INNER JOIN COMPANIES CO ON JAP.CompanyId = CO.CompanyId
            INNER JOIN TRANSACTION_STATUSES TS ON JAP.TransactionStatusId = TS.TransactionStatusId
            WHERE (@CompanyId IS NULL OR JAP.CompanyId = @CompanyId)
            AND (@TransactionStatusId IS NULL OR JAP.TransactionStatusId = @TransactionStatusId)
            ORDER BY 
                CASE WHEN @FieldToOrder = 'SeatNumber' AND @DirectionOrder = 'ASC' THEN JAP.Entry END ASC,
                CASE WHEN @FieldToOrder = 'SeatNumber' AND @DirectionOrder = 'DESC' THEN JAP.Entry END DESC,
                CASE WHEN @FieldToOrder = 'AccountingDate' AND @DirectionOrder = 'ASC' THEN JAP.AccountingDate END ASC,
                CASE WHEN @FieldToOrder = 'AccountingDate' AND @DirectionOrder = 'DESC' THEN JAP.AccountingDate END DESC,
                CASE WHEN @FieldToOrder = 'CompanyName' AND @DirectionOrder = 'ASC' THEN CO.Name END ASC,
                CASE WHEN @FieldToOrder = 'CompanyName' AND @DirectionOrder = 'DESC' THEN CO.Name END DESC,
                CASE WHEN @FieldToOrder = 'TransactionStatusName' AND @DirectionOrder = 'ASC' THEN TS.Name END ASC,
                CASE WHEN @FieldToOrder = 'TransactionStatusName' AND @DirectionOrder = 'DESC' THEN TS.Name END DESC,
                JAP.Entry
            OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;
            
            -- Asignamos el valor total de filas al parámetro de salida
            SET @TotalCount = (SELECT COUNT(*) FROM JOURNAL_ACCOUNTS_PAYABLE JAP2
                               WHERE (@CompanyId IS NULL OR JAP2.CompanyId = @CompanyId)
                               AND (@TransactionStatusId IS NULL OR JAP2.TransactionStatusId = @TransactionStatusId));
            END";

            migrationBuilder.Sql("DROP PROCEDURE IF SP_JOURNAL_ACCOUNTS_PAYABLE_GetAllJournalAccountsPayableWithFilters");
            migrationBuilder.Sql(spOriginal);
        }
    }
}
