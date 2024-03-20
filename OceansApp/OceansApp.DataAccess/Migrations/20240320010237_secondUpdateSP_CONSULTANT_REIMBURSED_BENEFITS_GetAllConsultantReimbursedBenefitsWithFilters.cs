using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    public partial class secondUpdateSP_CONSULTANT_REIMBURSED_BENEFITS_GetAllConsultantReimbursedBenefitsWithFilters : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //UPDATE STORED PROCEDURE
            var sp = @"CREATE PROCEDURE SP_CONSULTANT_REIMBURSED_BENEFITS_GetAllConsultantReimbursedBenefitsWithFilters
            @SearchText NVARCHAR(255),
            @StartDate DATE,
            @EndDate DATE,
            @BenefitPaid BIT,
            @BenefitId INT,
            @BenefitCategoryId INT,
            @FieldToOrder NVARCHAR(255),
            @DirectionOrder NVARCHAR(255),
            @Skip INT,
            @Take INT,
            @TotalCount INT OUTPUT
            AS
            BEGIN
            -- Count total results
            SELECT @TotalCount = COUNT(*)
            FROM CONSULTANT_REIMBURSED_BENEFITS CRB
            JOIN CONSULTANT_DETAILS CD ON CRB.ConsultantId = CD.ConsultantId
            JOIN Users U ON CD.UserId  = U.Id
            JOIN CONSULTANT_BENEFITS B ON CRB.BenefitId = B.BenefitId
            JOIN CONSULTANT_BENEFIT_CATEGORIES CBC ON CRB.BenefitCategoryId = CBC.BenefitCategoryId
            JOIN CONSULTANT_DETAILS CDC ON CRB.ConsultantIdCreatedBy = CDC.ConsultantId
            JOIN Users UserCreate ON CDC.UserId = UserCreate.Id
            LEFT JOIN CONSULTANT_DETAILS CDU ON CRB.ConsultantIdLastUpdatedBy = CDU.ConsultantId
            LEFT JOIN Users UserUpdate ON CDU.UserId = UserUpdate.Id
            WHERE (@SearchText IS NULL 
            OR LOWER(U.Name) LIKE '%' + LOWER(@SearchText) + '%'
            OR LOWER(U.LastName) LIKE '%' + LOWER(@SearchText) + '%'
            OR LOWER(CONCAT(U.Name, ' ', U.LastName)) LIKE '%' + LOWER(@SearchText) + '%'
	        OR LOWER(CRB.Detail) LIKE '%' + LOWER(@SearchText) + '%')
	        AND (@BenefitPaid IS NULL OR CRB.BenefitPaid = @BenefitPaid)
	        AND ((@StartDate IS NULL AND @EndDate IS NULL) OR (CRB.DateToBeReimbursed >= @StartDate AND CRB.DateToBeReimbursed <= @EndDate))
	        AND (@BenefitId IS NULL OR CRB.BenefitId = @BenefitId)
            AND (@BenefitCategoryId IS NULL OR CRB.BenefitCategoryId = @BenefitCategoryId);

            -- Request with pagination
            SELECT CRB.ReimbursedBenefitId
            ,U.Name + ' ' + U.LastName AS ConsultantName
	        ,B.Name AS BenefitName
            ,CBC.Name AS BenefitCategoryName
	        ,CRB.AmountReimbursed
	        ,CRB.Detail
            ,CRB.DateToBeReimbursed
            ,CRB.BenefitPaid
            ,UserCreate.Name + ' ' + UserCreate.LastName AS UserCreatedBy
	        ,CRB.CreationDate
	        ,UserUpdate.Name + ' ' + UserUpdate.LastName AS UserLastUpdatedBy
	        ,CRB.LastUpdateDate
            FROM CONSULTANT_REIMBURSED_BENEFITS CRB
            JOIN CONSULTANT_DETAILS CD ON CRB.ConsultantId = CD.ConsultantId
            JOIN Users U ON CD.UserId  = U.Id
            JOIN CONSULTANT_BENEFITS B ON CRB.BenefitId = B.BenefitId
            JOIN CONSULTANT_BENEFIT_CATEGORIES CBC ON CRB.BenefitCategoryId = CBC.BenefitCategoryId
            JOIN CONSULTANT_DETAILS CDC ON CRB.ConsultantIdCreatedBy = CDC.ConsultantId
            JOIN Users UserCreate ON CDC.UserId = UserCreate.Id
            LEFT JOIN CONSULTANT_DETAILS CDU ON CRB.ConsultantIdLastUpdatedBy = CDU.ConsultantId
            LEFT JOIN Users UserUpdate ON CDU.UserId = UserUpdate.Id
            WHERE (@SearchText IS NULL 
            OR LOWER(U.Name) LIKE '%' + LOWER(@SearchText) + '%'
            OR LOWER(U.LastName) LIKE '%' + LOWER(@SearchText) + '%'
            OR LOWER(CONCAT(U.Name, ' ', U.LastName)) LIKE '%' + LOWER(@SearchText) + '%'
	        OR LOWER(CRB.Detail) LIKE '%' + LOWER(@SearchText) + '%')
	        AND (@BenefitPaid IS NULL OR CRB.BenefitPaid = @BenefitPaid)
	        AND ((@StartDate IS NULL AND @EndDate IS NULL) OR (CRB.DateToBeReimbursed >= @StartDate AND CRB.DateToBeReimbursed <= @EndDate))
	        AND (@BenefitId IS NULL OR CRB.BenefitId = @BenefitId)
            AND (@BenefitCategoryId IS NULL OR CRB.BenefitCategoryId = @BenefitCategoryId)
	        ORDER BY 
            CASE WHEN @FieldToOrder = 'ConsultantName' AND @DirectionOrder = 'ASC' THEN U.Name END ASC,
            CASE WHEN @FieldToOrder = 'ConsultantName' AND @DirectionOrder = 'DESC' THEN U.Name END DESC,
		    CASE WHEN @FieldToOrder = 'BenefitName' AND @DirectionOrder = 'ASC' THEN B.Name END ASC,
            CASE WHEN @FieldToOrder = 'BenefitName' AND @DirectionOrder = 'DESC' THEN B.Name END DESC,
		    CASE WHEN @FieldToOrder = 'AmountReimbursed' AND @DirectionOrder = 'ASC' THEN CRB.AmountReimbursed END ASC,
            CASE WHEN @FieldToOrder = 'AmountReimbursed' AND @DirectionOrder = 'DESC' THEN CRB.AmountReimbursed END DESC,
		    CASE WHEN @FieldToOrder = 'DateToBeReimbursed' AND @DirectionOrder = 'ASC' THEN CRB.DateToBeReimbursed END ASC,
            CASE WHEN @FieldToOrder = 'DateToBeReimbursed' AND @DirectionOrder = 'DESC' THEN CRB.DateToBeReimbursed END DESC,
		    CASE WHEN @FieldToOrder = 'BenefitPaid' AND @DirectionOrder = 'ASC' THEN CRB.BenefitPaid END ASC,
            CASE WHEN @FieldToOrder = 'BenefitPaid' AND @DirectionOrder = 'DESC' THEN CRB.BenefitPaid END DESC,
		    CASE WHEN @FieldToOrder = 'UserCreatedBy' AND @DirectionOrder = 'ASC' THEN UserCreate.Name END ASC,
            CASE WHEN @FieldToOrder = 'UserCreatedBy' AND @DirectionOrder = 'DESC' THEN UserCreate.Name END DESC,
		    CASE WHEN @FieldToOrder = 'UserLastUpdatedBy' AND @DirectionOrder = 'ASC' THEN UserUpdate.Name END ASC,
            CASE WHEN @FieldToOrder = 'UserLastUpdatedBy' AND @DirectionOrder = 'DESC' THEN UserUpdate.Name END DESC,
		    CASE WHEN @FieldToOrder = 'CreationDate' AND @DirectionOrder = 'ASC' THEN CRB.CreationDate END ASC,
            CASE WHEN @FieldToOrder = 'CreationDate' AND @DirectionOrder = 'DESC' THEN CRB.CreationDate END DESC,
		    CASE WHEN @FieldToOrder = 'LastUpdatedDate' AND @DirectionOrder = 'ASC' THEN CRB.LastUpdateDate END ASC,
            CASE WHEN @FieldToOrder = 'LastUpdatedDate' AND @DirectionOrder = 'DESC' THEN CRB.LastUpdateDate END DESC,
            CASE WHEN @FieldToOrder = 'BenefitCategoryName' AND @DirectionOrder = 'ASC' THEN CBC.Name END ASC,
            CASE WHEN @FieldToOrder = 'BenefitCategoryName' AND @DirectionOrder = 'DESC' THEN CBC.Name END DESC,
            CRB.CreationDate DESC
            OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;
            END";

            // Delete stored procedure if exists
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS SP_CONSULTANT_REIMBURSED_BENEFITS_GetAllConsultantReimbursedBenefitsWithFilters");

            // Create new stored Procedure
            migrationBuilder.Sql(sp);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // restart the stored procedure to the original version
            var spOriginal = @"CREATE PROCEDURE SP_CONSULTANT_REIMBURSED_BENEFITS_GetAllConsultantReimbursedBenefitsWithFilters
            @SearchText NVARCHAR(255),
            @StartDate DATE,
            @EndDate DATE,
            @BenefitPaid BIT,
            @BenefitId INT,
            @FieldToOrder NVARCHAR(255),
            @DirectionOrder NVARCHAR(255),
            @Skip INT,
            @Take INT,
            @TotalCount INT OUTPUT
            AS
            BEGIN
            -- Count total results
            SELECT @TotalCount = COUNT(*)
            FROM CONSULTANT_REIMBURSED_BENEFITS CRB
            JOIN CONSULTANT_DETAILS CD ON CRB.ConsultantId = CD.ConsultantId
            JOIN Users U ON CD.UserId  = U.Id
            JOIN CONSULTANT_BENEFITS B ON CRB.BenefitId = B.BenefitId
            JOIN CONSULTANT_DETAILS CDC ON CRB.ConsultantIdCreatedBy = CDC.ConsultantId
            JOIN Users UserCreate ON CDC.UserId = UserCreate.Id
            LEFT JOIN CONSULTANT_DETAILS CDU ON CRB.ConsultantIdLastUpdatedBy = CDU.ConsultantId
            LEFT JOIN Users UserUpdate ON CDU.UserId = UserUpdate.Id
            WHERE (@SearchText IS NULL 
            OR LOWER(U.Name) LIKE '%' + LOWER(@SearchText) + '%'
            OR LOWER(U.LastName) LIKE '%' + LOWER(@SearchText) + '%'
            OR LOWER(CONCAT(U.Name, ' ', U.LastName)) LIKE '%' + LOWER(@SearchText) + '%'
	        OR LOWER(CRB.Detail) LIKE '%' + LOWER(@SearchText) + '%')
	        AND (@BenefitPaid IS NULL OR CRB.BenefitPaid = @BenefitPaid)
	        AND ((@StartDate IS NULL AND @EndDate IS NULL) OR (CRB.DateToBeReimbursed >= @StartDate AND CRB.DateToBeReimbursed <= @EndDate))
	        AND (@BenefitId IS NULL OR CRB.BenefitId = @BenefitId);

            -- Request with pagination
            SELECT CRB.ReimbursedBenefitId
            ,U.Name + ' ' + U.LastName AS ConsultantName
	        ,B.Name AS BenefitName
	        ,CRB.AmountReimbursed
	        ,CRB.Detail
            ,CRB.DateToBeReimbursed
            ,CRB.BenefitPaid
            ,UserCreate.Name + ' ' + UserCreate.LastName AS UserCreatedBy
	        ,CRB.CreationDate
	        ,UserUpdate.Name + ' ' + UserUpdate.LastName AS UserLastUpdatedBy
	        ,CRB.LastUpdateDate
            FROM CONSULTANT_REIMBURSED_BENEFITS CRB
            JOIN CONSULTANT_DETAILS CD ON CRB.ConsultantId = CD.ConsultantId
            JOIN Users U ON CD.UserId  = U.Id
            JOIN CONSULTANT_BENEFITS B ON CRB.BenefitId = B.BenefitId
            JOIN CONSULTANT_DETAILS CDC ON CRB.ConsultantIdCreatedBy = CDC.ConsultantId
            JOIN Users UserCreate ON CDC.UserId = UserCreate.Id
            LEFT JOIN CONSULTANT_DETAILS CDU ON CRB.ConsultantIdLastUpdatedBy = CDU.ConsultantId
            LEFT JOIN Users UserUpdate ON CDU.UserId = UserUpdate.Id
            WHERE (@SearchText IS NULL 
            OR LOWER(U.Name) LIKE '%' + LOWER(@SearchText) + '%'
            OR LOWER(U.LastName) LIKE '%' + LOWER(@SearchText) + '%'
            OR LOWER(CONCAT(U.Name, ' ', U.LastName)) LIKE '%' + LOWER(@SearchText) + '%'
	        OR LOWER(CRB.Detail) LIKE '%' + LOWER(@SearchText) + '%')
	        AND (@BenefitPaid IS NULL OR CRB.BenefitPaid = @BenefitPaid)
	        AND ((@StartDate IS NULL AND @EndDate IS NULL) OR (CRB.DateToBeReimbursed >= @StartDate AND CRB.DateToBeReimbursed <= @EndDate))
	        AND (@BenefitId IS NULL OR CRB.BenefitId = @BenefitId)
	        ORDER BY 
            CASE WHEN @FieldToOrder = 'ConsultantName' AND @DirectionOrder = 'ASC' THEN U.Name END ASC,
            CASE WHEN @FieldToOrder = 'ConsultantName' AND @DirectionOrder = 'DESC' THEN U.Name END DESC,
		    CASE WHEN @FieldToOrder = 'BenefitName' AND @DirectionOrder = 'ASC' THEN B.Name END ASC,
            CASE WHEN @FieldToOrder = 'BenefitName' AND @DirectionOrder = 'DESC' THEN B.Name END DESC,
		    CASE WHEN @FieldToOrder = 'AmountReimbursed' AND @DirectionOrder = 'ASC' THEN CRB.AmountReimbursed END ASC,
            CASE WHEN @FieldToOrder = 'AmountReimbursed' AND @DirectionOrder = 'DESC' THEN CRB.AmountReimbursed END DESC,
		    CASE WHEN @FieldToOrder = 'DateToBeReimbursed' AND @DirectionOrder = 'ASC' THEN CRB.DateToBeReimbursed END ASC,
            CASE WHEN @FieldToOrder = 'DateToBeReimbursed' AND @DirectionOrder = 'DESC' THEN CRB.DateToBeReimbursed END DESC,
		    CASE WHEN @FieldToOrder = 'BenefitPaid' AND @DirectionOrder = 'ASC' THEN CRB.BenefitPaid END ASC,
            CASE WHEN @FieldToOrder = 'BenefitPaid' AND @DirectionOrder = 'DESC' THEN CRB.BenefitPaid END DESC,
		    CASE WHEN @FieldToOrder = 'UserCreatedBy' AND @DirectionOrder = 'ASC' THEN UserCreate.Name END ASC,
            CASE WHEN @FieldToOrder = 'UserCreatedBy' AND @DirectionOrder = 'DESC' THEN UserCreate.Name END DESC,
		    CASE WHEN @FieldToOrder = 'UserLastUpdatedBy' AND @DirectionOrder = 'ASC' THEN UserUpdate.Name END ASC,
            CASE WHEN @FieldToOrder = 'UserLastUpdatedBy' AND @DirectionOrder = 'DESC' THEN UserUpdate.Name END DESC,
		    CASE WHEN @FieldToOrder = 'CreationDate' AND @DirectionOrder = 'ASC' THEN CRB.CreationDate END ASC,
            CASE WHEN @FieldToOrder = 'CreationDate' AND @DirectionOrder = 'DESC' THEN CRB.CreationDate END DESC,
		    CASE WHEN @FieldToOrder = 'LastUpdatedDate' AND @DirectionOrder = 'ASC' THEN CRB.LastUpdateDate END ASC,
            CASE WHEN @FieldToOrder = 'LastUpdatedDate' AND @DirectionOrder = 'DESC' THEN CRB.LastUpdateDate END DESC,
            CRB.CreationDate DESC
            OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;
            END";

            migrationBuilder.Sql("DROP PROCEDURE IF SP_CONSULTANT_REIMBURSED_BENEFITS_GetAllConsultantReimbursedBenefitsWithFilters");
            migrationBuilder.Sql(spOriginal);
        }
    }
}
