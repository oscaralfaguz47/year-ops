using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class addSP_JOURNAL_ACCOUNTS_PAYABLE_GetAllJournalAccountsPayableWithFilters : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
            migrationBuilder.Sql(sp);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS SP_JOURNAL_ACCOUNTS_PAYABLE_GetAllJournalAccountsPayableWithFilters");
        }
    }
}
