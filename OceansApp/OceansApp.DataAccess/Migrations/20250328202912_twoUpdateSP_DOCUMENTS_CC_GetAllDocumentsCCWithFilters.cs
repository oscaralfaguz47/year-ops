using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class twoUpdateSP_DOCUMENTS_CC_GetAllDocumentsCCWithFilters : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //UPDATE STORED PROCEDURE
            var sp = @"CREATE PROCEDURE SP_DOCUMENTS_CC_GetAllDocumentsCCWithFilters
            @SearchText NVARCHAR(255),
            @DocumentType   NVARCHAR(4),
            @ClientId       INT,
            @CompanyId      NVARCHAR(5),
            @StartDate      DATE,
            @EndDate        DATE,
            @Skip           INT,
            @Take           INT,
            @FieldToOrder   NVARCHAR(255),
            @DirectionOrder NVARCHAR(255),
            @TotalCount INT OUTPUT
            AS
            BEGIN
            SET NOCOUNT ON;
                        SELECT 
                DCC.DocumentCCId,
                DCC.DocumentNumber,
                DCC.DocumentType,
                DCC.ApplicationDescription,
                DCC.DocumentDate,
                DATEADD(DAY, CAST(C.PaymentCondition AS INT), DCC.DocumentDate) AS ExpirationDate,
                CASE
                    WHEN DCC.BalanceAmount > 0 THEN 
                        DATEDIFF(DAY, SWITCHOFFSET(SYSDATETIMEOFFSET(), '-06:00'), 
                                 DATEADD(DAY, CAST(C.PaymentCondition AS INT), DCC.DocumentDate))
                    ELSE 0
                END AS NumDaysToExpire,
                DCC.BalanceAmount,
                DCC.DocumentAmount,
                DCC.Canceled,
                C.Name AS ClientName,
                DCC.CompanyId,
                C.ClientCategory,
                (
                    SELECT COUNT(*) 
                    FROM DOCUMENTS_CC_NOTIFICATIONS N 
                    WHERE N.DocumentCCId = DCC.DocumentCCId
                ) AS NumNotificationsSent
            INTO #FilteredDocuments
            FROM DOCUMENTS_CC DCC
            INNER JOIN CLIENT C ON DCC.ClientId = C.ClientId
            WHERE 
                (
                    @SearchText IS NULL OR
                    LOWER(DCC.DocumentNumber) LIKE '%' + LOWER(@SearchText) + '%' OR
                    LOWER(DCC.ApplicationDescription) LIKE '%' + LOWER(@SearchText) + '%'
                )
                AND (@ClientId IS NULL OR DCC.ClientId = @ClientId)
                AND (@CompanyId IS NULL OR DCC.CompanyId = @CompanyId)
                AND (@DocumentType IS NULL OR DCC.DocumentType = @DocumentType)
                AND (
                    (@StartDate IS NULL AND @EndDate IS NULL) OR
                    (DCC.DocumentDate >= @StartDate AND DCC.DocumentDate <= @EndDate)
                );
            
            -- ======================================
            -- Return Ordered and Paginated Results
            -- ======================================
            SELECT *
            FROM #FilteredDocuments
            ORDER BY 
                CASE WHEN @FieldToOrder = 'DocumentNumber' AND @DirectionOrder = 'ASC' THEN DocumentNumber END ASC,
                CASE WHEN @FieldToOrder = 'DocumentNumber' AND @DirectionOrder = 'DESC' THEN DocumentNumber END DESC,
                CASE WHEN @FieldToOrder = 'DocumentType' AND @DirectionOrder = 'ASC' THEN DocumentType END ASC,
                CASE WHEN @FieldToOrder = 'DocumentType' AND @DirectionOrder = 'DESC' THEN DocumentType END DESC,
                CASE WHEN @FieldToOrder = 'ApplicationDescription' AND @DirectionOrder = 'ASC' THEN ApplicationDescription END ASC,
                CASE WHEN @FieldToOrder = 'ApplicationDescription' AND @DirectionOrder = 'DESC' THEN ApplicationDescription END DESC,
                CASE WHEN @FieldToOrder = 'DocumentDate' AND @DirectionOrder = 'ASC' THEN DocumentDate END ASC,
                CASE WHEN @FieldToOrder = 'DocumentDate' AND @DirectionOrder = 'DESC' THEN DocumentDate END DESC,
                CASE WHEN @FieldToOrder = 'ExpirationDate' AND @DirectionOrder = 'ASC' THEN ExpirationDate END ASC,
                CASE WHEN @FieldToOrder = 'ExpirationDate' AND @DirectionOrder = 'DESC' THEN ExpirationDate END DESC,
                CASE WHEN @FieldToOrder = 'NumDaysToExpire' AND @DirectionOrder = 'ASC' THEN NumDaysToExpire END ASC,
                CASE WHEN @FieldToOrder = 'NumDaysToExpire' AND @DirectionOrder = 'DESC' THEN NumDaysToExpire END DESC,
                CASE WHEN @FieldToOrder = 'BalanceAmount' AND @DirectionOrder = 'ASC' THEN BalanceAmount END ASC,
                CASE WHEN @FieldToOrder = 'BalanceAmount' AND @DirectionOrder = 'DESC' THEN BalanceAmount END DESC,
                CASE WHEN @FieldToOrder = 'DocumentAmount' AND @DirectionOrder = 'ASC' THEN DocumentAmount END ASC,
                CASE WHEN @FieldToOrder = 'DocumentAmount' AND @DirectionOrder = 'DESC' THEN DocumentAmount END DESC,
                CASE WHEN @FieldToOrder = 'ClientName' AND @DirectionOrder = 'ASC' THEN ClientName END ASC,
                CASE WHEN @FieldToOrder = 'ClientName' AND @DirectionOrder = 'DESC' THEN ClientName END DESC,
                CASE WHEN @FieldToOrder = 'ClientCategory' AND @DirectionOrder = 'ASC' THEN ClientCategory END ASC,
                CASE WHEN @FieldToOrder = 'ClientCategory' AND @DirectionOrder = 'DESC' THEN ClientCategory END DESC,
            	DocumentType, NumDaysToExpire
            OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;
            
            -- ======================================
            -- Count Total Matching Rows
            -- ======================================
            SELECT @TotalCount = COUNT(*) FROM #FilteredDocuments;
            
            DROP TABLE #FilteredDocuments;
        END;";

            // Delete stored procedure if exists
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS SP_DOCUMENTS_CC_GetAllDocumentsCCWithFilters");

            // Create new stored Procedure
            migrationBuilder.Sql(sp);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // restart the stored procedure to the original version
            var spOriginal = @"CREATE PROCEDURE SP_DOCUMENTS_CC_GetAllDocumentsCCWithFilters
            @SearchText NVARCHAR(255),
            @DocumentType   NVARCHAR(4),
            @ClientId       INT,
            @CompanyId      NVARCHAR(5),
            @StartDate      DATE,
            @EndDate        DATE,
            @Skip           INT,
            @Take           INT,
            @FieldToOrder   NVARCHAR(255),
            @DirectionOrder NVARCHAR(255)
            AS
            BEGIN
            SET NOCOUNT ON;
                        SELECT 
                DCC.DocumentCCId,
                DCC.DocumentNumber,
                DCC.DocumentType,
                DCC.ApplicationDescription,
                DCC.DocumentDate,
                DATEADD(DAY, CAST(C.PaymentCondition AS INT), DCC.DocumentDate) AS ExpirationDate,
                CASE
                    WHEN DCC.BalanceAmount > 0 THEN 
                        DATEDIFF(DAY, SWITCHOFFSET(SYSDATETIMEOFFSET(), '-06:00'), 
                                 DATEADD(DAY, CAST(C.PaymentCondition AS INT), DCC.DocumentDate))
                    ELSE 0
                END AS NumDaysToExpire,
                DCC.BalanceAmount,
                DCC.DocumentAmount,
                DCC.Canceled,
                C.Name AS ClientName,
                DCC.CompanyId,
                C.ClientCategory,
                (
                    SELECT COUNT(*) 
                    FROM DOCUMENTS_CC_NOTIFICATIONS N 
                    WHERE N.DocumentCCId = DCC.DocumentCCId
                ) AS NumNotificationsSent
            INTO #FilteredDocuments
            FROM DOCUMENTS_CC DCC
            INNER JOIN CLIENT C ON DCC.ClientId = C.ClientId
            WHERE 
                (
                    @SearchText IS NULL OR
                    LOWER(DCC.DocumentNumber) LIKE '%' + LOWER(@SearchText) + '%' OR
                    LOWER(DCC.ApplicationDescription) LIKE '%' + LOWER(@SearchText) + '%'
                )
                AND (@ClientId IS NULL OR DCC.ClientId = @ClientId)
                AND (@CompanyId IS NULL OR DCC.CompanyId = @CompanyId)
                AND (@DocumentType IS NULL OR DCC.DocumentType = @DocumentType)
                AND (
                    (@StartDate IS NULL AND @EndDate IS NULL) OR
                    (DCC.DocumentDate >= @StartDate AND DCC.DocumentDate <= @EndDate)
                );
            
            -- ======================================
            -- Return Ordered and Paginated Results
            -- ======================================
            SELECT *
            FROM #FilteredDocuments
            ORDER BY 
                CASE WHEN @FieldToOrder = 'DocumentNumber' AND @DirectionOrder = 'ASC' THEN DocumentNumber END ASC,
                CASE WHEN @FieldToOrder = 'DocumentNumber' AND @DirectionOrder = 'DESC' THEN DocumentNumber END DESC,
                CASE WHEN @FieldToOrder = 'DocumentType' AND @DirectionOrder = 'ASC' THEN DocumentType END ASC,
                CASE WHEN @FieldToOrder = 'DocumentType' AND @DirectionOrder = 'DESC' THEN DocumentType END DESC,
                CASE WHEN @FieldToOrder = 'ApplicationDescription' AND @DirectionOrder = 'ASC' THEN ApplicationDescription END ASC,
                CASE WHEN @FieldToOrder = 'ApplicationDescription' AND @DirectionOrder = 'DESC' THEN ApplicationDescription END DESC,
                CASE WHEN @FieldToOrder = 'DocumentDate' AND @DirectionOrder = 'ASC' THEN DocumentDate END ASC,
                CASE WHEN @FieldToOrder = 'DocumentDate' AND @DirectionOrder = 'DESC' THEN DocumentDate END DESC,
                CASE WHEN @FieldToOrder = 'ExpirationDate' AND @DirectionOrder = 'ASC' THEN ExpirationDate END ASC,
                CASE WHEN @FieldToOrder = 'ExpirationDate' AND @DirectionOrder = 'DESC' THEN ExpirationDate END DESC,
                CASE WHEN @FieldToOrder = 'NumDaysToExpire' AND @DirectionOrder = 'ASC' THEN NumDaysToExpire END ASC,
                CASE WHEN @FieldToOrder = 'NumDaysToExpire' AND @DirectionOrder = 'DESC' THEN NumDaysToExpire END DESC,
                CASE WHEN @FieldToOrder = 'BalanceAmount' AND @DirectionOrder = 'ASC' THEN BalanceAmount END ASC,
                CASE WHEN @FieldToOrder = 'BalanceAmount' AND @DirectionOrder = 'DESC' THEN BalanceAmount END DESC,
                CASE WHEN @FieldToOrder = 'DocumentAmount' AND @DirectionOrder = 'ASC' THEN DocumentAmount END ASC,
                CASE WHEN @FieldToOrder = 'DocumentAmount' AND @DirectionOrder = 'DESC' THEN DocumentAmount END DESC,
                CASE WHEN @FieldToOrder = 'ClientName' AND @DirectionOrder = 'ASC' THEN ClientName END ASC,
                CASE WHEN @FieldToOrder = 'ClientName' AND @DirectionOrder = 'DESC' THEN ClientName END DESC,
                CASE WHEN @FieldToOrder = 'ClientCategory' AND @DirectionOrder = 'ASC' THEN ClientCategory END ASC,
                CASE WHEN @FieldToOrder = 'ClientCategory' AND @DirectionOrder = 'DESC' THEN ClientCategory END DESC,
            	DocumentType, NumDaysToExpire
            OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;
            
            -- ======================================
            -- Count Total Matching Rows
            -- ======================================
            SELECT COUNT(*) AS TotalCount
            FROM #FilteredDocuments;
            
            DROP TABLE #FilteredDocuments;
        END;";

            migrationBuilder.Sql("DROP PROCEDURE IF SP_DOCUMENTS_CC_GetAllDocumentsCCWithFilters");
            migrationBuilder.Sql(spOriginal);
        }
    }
}
