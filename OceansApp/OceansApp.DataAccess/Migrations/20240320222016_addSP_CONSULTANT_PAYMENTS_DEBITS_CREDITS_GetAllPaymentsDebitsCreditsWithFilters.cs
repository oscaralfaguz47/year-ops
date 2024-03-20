using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    public partial class addSP_CONSULTANT_PAYMENTS_DEBITS_CREDITS_GetAllPaymentsDebitsCreditsWithFilters : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"CREATE PROCEDURE SP_CONSULTANT_PAYMENTS_DEBITS_CREDITS_GetAllPaymentsDebitsCreditsWithFilters
            @SearchText NVARCHAR(255),
            @StartDate DATE,
            @EndDate DATE,
            @TransactionStatusId INT,
            @TransactionTypeId INT,
            @FieldToOrder NVARCHAR(255),
            @DirectionOrder NVARCHAR(255),
            @Skip INT,
            @Take INT,
            @TotalCount INT OUTPUT
            AS
            BEGIN
            -- Count total results
            SELECT @TotalCount = COUNT(*)
            FROM 
            CONSULTANT_PAYMENTS_DEBITS_CREDITS CPDC
            INNER JOIN CONSULTANT_DETAILS CD ON CPDC.ConsultantId = CD.ConsultantId
            INNER JOIN Users UC ON CD.UserId = UC.Id
            INNER JOIN COST_CENTER CC ON CPDC.CostCenterId = CC.CostCenterId
            INNER JOIN ACCOUNTING_ACCOUNT AA ON CPDC.AccountingAccountId = AA.AccountingAccountId
            INNER JOIN TRANSACTION_STATUSES TS ON CPDC.TransactionStatusId = TS.TransactionStatusId
            INNER JOIN TRANSACTION_TYPES TT ON CPDC.TransactionTypeId = TT.TransactionTypeId
            INNER JOIN CONSULTANT_DETAILS CCreate ON CPDC.ConsultantIdCreatedBy = CCreate.ConsultantId
            INNER JOIN Users UCr ON CCreate.UserId = UCr.Id
            INNER JOIN CONSULTANT_DETAILS CUpdate ON CPDC.ConsultantIdLastUpdatedBy = CUpdate.ConsultantId
            INNER JOIN Users UUp ON CUpdate.UserId = UUp.Id
	        WHERE (@SearchText IS NULL 
            OR LOWER(UC.Name) LIKE '%' + LOWER(@SearchText) + '%'
			OR LOWER(UC.LastName) LIKE '%' + LOWER(@SearchText) + '%'
			OR LOWER(CONCAT(UC.Name, ' ', UC.LastName)) LIKE '%' + LOWER(@SearchText) + '%'
			OR LOWER(CPDC.Detail) LIKE '%' + LOWER(@SearchText) + '%'
			OR LOWER(CC.Description) LIKE '%' + LOWER(@SearchText) + '%'
			OR LOWER(AA.Description) LIKE '%' + LOWER(@SearchText) + '%')
			AND (@TransactionStatusId IS NULL OR CPDC.TransactionStatusId = @TransactionStatusId)
			AND (@TransactionTypeId IS NULL OR CPDC.TransactionTypeId = @TransactionTypeId)
			AND ((@StartDate IS NULL AND @EndDate IS NULL) OR (CPDC.ActionDateWithinFortnight >= @StartDate AND CPDC.ActionDateWithinFortnight <= @EndDate));

            -- Request with pagination
            SELECT 
            CPDC.ConsultantPaymentDebitsCreditsId,
            UC.Name + ' ' + UC.LastName AS ConsultantName,
            CC.Description AS CostCenterName,
            AA.Description AS AccountingAccountName,
            CPDC.Detail,
            CPDC.Amount,
            CPDC.ActionDateWithinFortnight,
            TS.Name AS TransactionStatusName,
            TT.Name AS TransactionTypeName,
            CPDC.CreationDate,
            UCr.Name + ' ' + UCr.LastName AS UserCreatedBy,
            CPDC.LastUpdateDate,
            UUp.Name + ' ' + UUp.LastName AS LastUpdatedBy
            FROM 
            CONSULTANT_PAYMENTS_DEBITS_CREDITS CPDC
            INNER JOIN CONSULTANT_DETAILS CD ON CPDC.ConsultantId = CD.ConsultantId
            INNER JOIN Users UC ON CD.UserId = UC.Id
            INNER JOIN COST_CENTER CC ON CPDC.CostCenterId = CC.CostCenterId
            INNER JOIN ACCOUNTING_ACCOUNT AA ON CPDC.AccountingAccountId = AA.AccountingAccountId
            INNER JOIN TRANSACTION_STATUSES TS ON CPDC.TransactionStatusId = TS.TransactionStatusId
            INNER JOIN TRANSACTION_TYPES TT ON CPDC.TransactionTypeId = TT.TransactionTypeId
            INNER JOIN CONSULTANT_DETAILS CCreate ON CPDC.ConsultantIdCreatedBy = CCreate.ConsultantId
            INNER JOIN Users UCr ON CCreate.UserId = UCr.Id
            INNER JOIN CONSULTANT_DETAILS CUpdate ON CPDC.ConsultantIdLastUpdatedBy = CUpdate.ConsultantId
            INNER JOIN Users UUp ON CUpdate.UserId = UUp.Id
	        WHERE (@SearchText IS NULL 
            OR LOWER(UC.Name) LIKE '%' + LOWER(@SearchText) + '%'
			OR LOWER(UC.LastName) LIKE '%' + LOWER(@SearchText) + '%'
			OR LOWER(CONCAT(UC.Name, ' ', UC.LastName)) LIKE '%' + LOWER(@SearchText) + '%'
			OR LOWER(CPDC.Detail) LIKE '%' + LOWER(@SearchText) + '%'
			OR LOWER(CC.Description) LIKE '%' + LOWER(@SearchText) + '%'
			OR LOWER(AA.Description) LIKE '%' + LOWER(@SearchText) + '%')
			AND (@TransactionStatusId IS NULL OR CPDC.TransactionStatusId = @TransactionStatusId)
			AND (@TransactionTypeId IS NULL OR CPDC.TransactionTypeId = @TransactionTypeId)
			AND ((@StartDate IS NULL AND @EndDate IS NULL) OR (CPDC.ActionDateWithinFortnight >= @StartDate AND CPDC.ActionDateWithinFortnight <= @EndDate))
			ORDER BY 
            CASE WHEN @FieldToOrder = 'ConsultantName' AND @DirectionOrder = 'ASC' THEN UC.Name END ASC,
            CASE WHEN @FieldToOrder = 'ConsultantName' AND @DirectionOrder = 'DESC' THEN UC.Name END DESC,
			CASE WHEN @FieldToOrder = 'CostCenterName' AND @DirectionOrder = 'ASC' THEN CC.Description END ASC,
            CASE WHEN @FieldToOrder = 'CostCenterName' AND @DirectionOrder = 'DESC' THEN CC.Description END DESC,
			CASE WHEN @FieldToOrder = 'AccountingAccountName' AND @DirectionOrder = 'ASC' THEN AA.Description END ASC,
            CASE WHEN @FieldToOrder = 'AccountingAccountName' AND @DirectionOrder = 'DESC' THEN AA.Description END DESC,
			CASE WHEN @FieldToOrder = 'Amount' AND @DirectionOrder = 'ASC' THEN CPDC.Amount END ASC,
            CASE WHEN @FieldToOrder = 'Amount' AND @DirectionOrder = 'DESC' THEN CPDC.Amount END DESC,
			CASE WHEN @FieldToOrder = 'ActionDateWithinFortnight' AND @DirectionOrder = 'ASC' THEN CPDC.ActionDateWithinFortnight END ASC,
            CASE WHEN @FieldToOrder = 'ActionDateWithinFortnight' AND @DirectionOrder = 'DESC' THEN CPDC.ActionDateWithinFortnight END DESC,
			CASE WHEN @FieldToOrder = 'TransactionStatusName' AND @DirectionOrder = 'ASC' THEN TS.Name END ASC,
            CASE WHEN @FieldToOrder = 'TransactionStatusName' AND @DirectionOrder = 'DESC' THEN TS.Name END DESC,
			CASE WHEN @FieldToOrder = 'TransactionTypeName' AND @DirectionOrder = 'ASC' THEN TT.Name END ASC,
            CASE WHEN @FieldToOrder = 'TransactionTypeName' AND @DirectionOrder = 'DESC' THEN TT.Name END DESC,
			CASE WHEN @FieldToOrder = 'CreationDate' AND @DirectionOrder = 'ASC' THEN CPDC.CreationDate END ASC,
            CASE WHEN @FieldToOrder = 'CreationDate' AND @DirectionOrder = 'DESC' THEN CPDC.CreationDate END DESC,
			CASE WHEN @FieldToOrder = 'LastUpdatedDate' AND @DirectionOrder = 'ASC' THEN CPDC.LastUpdateDate END ASC,
            CASE WHEN @FieldToOrder = 'LastUpdatedDate' AND @DirectionOrder = 'DESC' THEN CPDC.LastUpdateDate END DESC,
			CASE WHEN @FieldToOrder = 'UserCreatedBy' AND @DirectionOrder = 'ASC' THEN UCr.Name END ASC,
            CASE WHEN @FieldToOrder = 'UserCreatedBy' AND @DirectionOrder = 'DESC' THEN UCr.Name END DESC,
		    CASE WHEN @FieldToOrder = 'LastUpdatedBy' AND @DirectionOrder = 'ASC' THEN UUp.Name END ASC,
            CASE WHEN @FieldToOrder = 'LastUpdatedBy' AND @DirectionOrder = 'DESC' THEN UUp.Name END DESC,
            CPDC.CreationDate DESC
            OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;
            END";
            migrationBuilder.Sql(sp);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS SP_CONSULTANT_PAYMENTS_DEBITS_CREDITS_GetAllPaymentsDebitsCreditsWithFilters");
        }
    }
}
