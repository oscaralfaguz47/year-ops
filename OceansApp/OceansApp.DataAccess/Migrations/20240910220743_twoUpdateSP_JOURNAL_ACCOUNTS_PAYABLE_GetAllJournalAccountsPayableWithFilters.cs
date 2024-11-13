using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class twoUpdateSP_JOURNAL_ACCOUNTS_PAYABLE_GetAllJournalAccountsPayableWithFilters : Migration
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
                ;WITH JournalCTE AS (
                    SELECT 
                          JAP.Entry as SeatNumber,
                          JAP.AccountingDate,
                          JAP.StartDatePeriod,
                          JAP.EndDatePeriod,
                          CO.Name AS CompanyName,
                          TS.Name AS TransactionStatusName,
                          COUNT(*) OVER () AS TotalCount 
                    FROM JOURNAL_ACCOUNTS_PAYABLE JAP
                    INNER JOIN COMPANIES CO ON JAP.CompanyId = CO.CompanyId
                    INNER JOIN TRANSACTION_STATUSES TS ON JAP.TransactionStatusId = TS.TransactionStatusId
                    WHERE (@CompanyId IS NULL OR JAP.CompanyId = @CompanyId)
                    AND (@TransactionStatusId IS NULL OR JAP.TransactionStatusId = @TransactionStatusId)
                )
                SELECT 
                    SeatNumber,
                    AccountingDate,
                    StartDatePeriod,
                    EndDatePeriod,
                    CompanyName,
                    TransactionStatusName
                FROM JournalCTE
                ORDER BY 
                    CASE WHEN @FieldToOrder = 'SeatNumber' AND @DirectionOrder = 'ASC' THEN SeatNumber END ASC,
                    CASE WHEN @FieldToOrder = 'SeatNumber' AND @DirectionOrder = 'DESC' THEN SeatNumber END DESC,
                    CASE WHEN @FieldToOrder = 'AccountingDate' AND @DirectionOrder = 'ASC' THEN AccountingDate END ASC,
                    CASE WHEN @FieldToOrder = 'AccountingDate' AND @DirectionOrder = 'DESC' THEN AccountingDate END DESC,
                    CASE WHEN @FieldToOrder = 'CompanyName' AND @DirectionOrder = 'ASC' THEN CompanyName END ASC,
                    CASE WHEN @FieldToOrder = 'CompanyName' AND @DirectionOrder = 'DESC' THEN CompanyName END DESC,
                    CASE WHEN @FieldToOrder = 'TransactionStatusName' AND @DirectionOrder = 'ASC' THEN TransactionStatusName END ASC,
                    CASE WHEN @FieldToOrder = 'TransactionStatusName' AND @DirectionOrder = 'DESC' THEN TransactionStatusName END DESC,
                    SeatNumber
                OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;
            
                SET @TotalCount = (SELECT TOP 1 TotalCount FROM JournalCTE);
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
            SELECT @TotalCount = COUNT(*)
            FROM JOURNAL_ACCOUNTS_PAYABLE JAP
            INNER JOIN COMPANIES CO ON JAP.CompanyId = CO.CompanyId
            INNER JOIN TRANSACTION_STATUSES TS ON JAP.TransactionStatusId = TS.TransactionStatusId
            WHERE (@CompanyId IS NULL OR JAP.CompanyId = @CompanyId)
            AND (@TransactionStatusId IS NULL OR JAP.TransactionStatusId = @TransactionStatusId);

            SELECT 
              JAP.Entry as SeatNumber
        	  ,JAP.AccountingDate
        	  ,StartDatePeriod
              ,EndDatePeriod
              ,CO.Name AS CompanyName
        	  ,TS.Name AS TransactionStatusName
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
          CASE WHEN @FieldToOrder = 'TransactionStatusName' AND @DirectionOrder = 'ASC' THEN JAP.TransactionStatusId END ASC,
          CASE WHEN @FieldToOrder = 'TransactionStatusName' AND @DirectionOrder = 'DESC' THEN JAP.TransactionStatusId END DESC,
          JAP.Entry
          OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;
          END";

            migrationBuilder.Sql("DROP PROCEDURE IF SP_JOURNAL_ACCOUNTS_PAYABLE_GetAllJournalAccountsPayableWithFilters");
            migrationBuilder.Sql(spOriginal);
        }
    }
}
