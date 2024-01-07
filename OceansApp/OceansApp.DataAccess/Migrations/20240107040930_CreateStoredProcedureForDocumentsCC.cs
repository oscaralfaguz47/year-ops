using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    public partial class CreateStoredProcedureForDocumentsCC : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"CREATE PROCEDURE GetAllDocumentsCCWithFilters
    @SearchText NVARCHAR(MAX),
    @DocumentType NVARCHAR(MAX),
    @ClientId INT,
    @CompanyId NVARCHAR(MAX),
    @StartDate DATE,
    @EndDate DATE,
    @Skip INT,
    @Take INT
AS
BEGIN
    SET NOCOUNT ON;

    -- Consulta principal
    SELECT DocumentCCId
        ,DocumentNumber
        ,DCC.DocumentType
        ,DCC.ApplicationDescription
        ,DCC.DocumentDate
        ,DATEADD(DAY, CAST(C.PaymentCondition AS INT), DCC.DocumentDate) AS ExpirationDate
        ,CASE
            WHEN DCC.BalanceAmount > 0 THEN DATEDIFF(DAY, SWITCHOFFSET(SYSDATETIMEOFFSET(), '-06:00'), 
                (DATEADD(DAY, CAST(C.PaymentCondition AS INT), DCC.DocumentDate))) 
            ELSE 0
            END AS NumDaysToExpire
        ,DCC.BalanceAmount
        ,DCC.DocumentAmount
        ,DCC.Canceled
        ,C.Name AS ClientName
        ,DCC.CompanyId
        ,C.ClientCategory
        ,(SELECT COUNT(*) FROM DOCUMENTS_CC_NOTIFICATIONS WHERE DocumentCCId = DCC.DocumentCCId) AS NumNotificationsSent
    FROM DOCUMENTS_CC DCC
    JOIN CLIENT C ON DCC.ClientId = C.ClientId
    WHERE ((@SearchText IS NULL OR LOWER(DCC.DocumentNumber) LIKE '%' + LOWER(@SearchText) + '%')
        OR (@SearchText IS NULL OR LOWER(DCC.ApplicationDescription) LIKE '%' + LOWER(@SearchText) + '%'))
        AND (@ClientId IS NULL OR DCC.ClientId = @ClientId)
        AND (@CompanyId IS NULL OR DCC.CompanyId = @CompanyId)
        AND (@DocumentType IS NULL OR DCC.DocumentType = @DocumentType)
        AND ((@StartDate IS NULL AND @EndDate IS NULL) OR (DCC.DocumentDate >= @StartDate AND DCC.DocumentDate <= @EndDate))
    ORDER BY DCC.DocumentType, NumDaysToExpire ASC
    OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;

    -- Consulta para contar total de filas
    SELECT COUNT(*) AS TotalCount
    FROM DOCUMENTS_CC DCC
    JOIN CLIENT C ON DCC.ClientId = C.ClientId
    WHERE ((@SearchText IS NULL OR LOWER(DCC.DocumentNumber) LIKE '%' + LOWER(@SearchText) + '%')
        OR (@SearchText IS NULL OR LOWER(DCC.ApplicationDescription) LIKE '%' + LOWER(@SearchText) + '%'))
        AND (@ClientId IS NULL OR DCC.ClientId = @ClientId)
        AND (@CompanyId IS NULL OR DCC.CompanyId = @CompanyId)
        AND (@DocumentType IS NULL OR DCC.DocumentType = @DocumentType)
        AND ((@StartDate IS NULL AND @EndDate IS NULL) OR (DCC.DocumentDate >= @StartDate AND DCC.DocumentDate <= @EndDate));
END
";
            migrationBuilder.Sql(sp);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS GetAllDocumentsCCWithFilters");
        }
    }
}
