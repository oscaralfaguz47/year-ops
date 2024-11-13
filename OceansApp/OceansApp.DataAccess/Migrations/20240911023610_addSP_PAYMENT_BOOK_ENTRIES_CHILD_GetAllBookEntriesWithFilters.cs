using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class addSP_PAYMENT_BOOK_ENTRIES_CHILD_GetAllBookEntriesWithFilters : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"CREATE PROCEDURE SP_PAYMENT_BOOK_ENTRIES_CHILD_GetAllBookEntriesWithFilters
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
        PBE.ParentId,
	    PBE.CreationDate,
        CO.Name AS CompanyName,
        TS.Name AS TransactionStatusName,
        SUM(CASE WHEN PBC.Voided = 0 THEN 1 ELSE 0 END) AS NumValidChildren,
        SUM(CASE WHEN PBC.Voided = 1 THEN 1 ELSE 0 END) AS NumVoidedChildren,
        COUNT(*) OVER() AS TotalCount
        FROM PAYMENT_BOOK_ENTRIES_PARENT PBE
        INNER JOIN COMPANIES CO ON PBE.CompanyId = CO.CompanyId
        INNER JOIN TRANSACTION_STATUSES TS ON PBE.TransactionStatusId = TS.TransactionStatusId
        LEFT JOIN PAYMENT_BOOK_ENTRIES_CHILD PBC ON PBE.ParentId = PBC.ParentId 
        WHERE (@CompanyId IS NULL OR PBE.CompanyId = @CompanyId)
        AND (@TransactionStatusId IS NULL OR PBE.TransactionStatusId = @TransactionStatusId)
        GROUP BY 
            PBE.ParentId,
        	PBE.CreationDate,
            CO.Name,
            TS.Name
        ORDER BY 
            CASE WHEN @FieldToOrder = 'ParentId' AND @DirectionOrder = 'ASC' THEN PBE.ParentId END ASC,
            CASE WHEN @FieldToOrder = 'ParentId' AND @DirectionOrder = 'DESC' THEN PBE.ParentId END DESC,
            CASE WHEN @FieldToOrder = 'CreationDate' AND @DirectionOrder = 'ASC' THEN PBE.CreationDate END ASC,
            CASE WHEN @FieldToOrder = 'CreationDate' AND @DirectionOrder = 'DESC' THEN PBE.CreationDate END DESC,
            CASE WHEN @FieldToOrder = 'CompanyName' AND @DirectionOrder = 'ASC' THEN CO.Name END ASC,
            CASE WHEN @FieldToOrder = 'CompanyName' AND @DirectionOrder = 'DESC' THEN CO.Name END DESC,
            CASE WHEN @FieldToOrder = 'TransactionStatusName' AND @DirectionOrder = 'ASC' THEN TS.Name END ASC,
            CASE WHEN @FieldToOrder = 'TransactionStatusName' AND @DirectionOrder = 'DESC' THEN TS.Name END DESC,
            PBE.ParentId DESC
        OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;
        
        SET @TotalCount = (SELECT COUNT(*) FROM PAYMENT_BOOK_ENTRIES_PARENT PBE2
                           WHERE (@CompanyId IS NULL OR PBE2.CompanyId = @CompanyId)
                           AND (@TransactionStatusId IS NULL OR PBE2.TransactionStatusId = @TransactionStatusId));
        END";
            migrationBuilder.Sql(sp);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS SP_PAYMENT_BOOK_ENTRIES_CHILD_GetAllBookEntriesWithFilters");
        }
    }
}
