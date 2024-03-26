using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    public partial class secondUpdateSP_INTERVIEWS_GetAllInterviewsWithFilters : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //UPDATE STORED PROCEDURE
            var sp = @"CREATE PROCEDURE SP_INTERVIEWS_GetAllInterviewsWithFilters
            @SearchText NVARCHAR(255),
            @StartDate DATE,
            @EndDate DATE,
            @TransactionStatusId INT,
            @FieldToOrder NVARCHAR(255),
            @DirectionOrder NVARCHAR(255),
            @Skip INT,
            @Take INT,
            @TotalCount INT OUTPUT
            AS
            BEGIN
            -- Count total results
            SELECT @TotalCount = COUNT(*)
            FROM INTERVIEWS I
            INNER JOIN TRANSACTION_STATUSES TS ON I.TransactionStatusId = TS.TransactionStatusId
            INNER JOIN CONSULTANT_DETAILS CD ON I.ConsultantId = CD.ConsultantId
            INNER JOIN Users Uc ON CD.UserId = Uc.Id
            INNER JOIN CONSULTANT_DETAILS CC ON I.ConsultantIdCreatedBy = CC.ConsultantId
            INNER JOIN Users Ucreate ON CC.UserId = Ucreate.Id
            LEFT JOIN CONSULTANT_DETAILS CU ON I.ConsultantIdLastUpdatedBy = CU.ConsultantId
            LEFT JOIN Users Uupdate ON CU.UserId = Uupdate.Id
            WHERE (@SearchText IS NULL 
            OR LOWER(Uc.Name) LIKE '%' + LOWER(@SearchText) + '%'
			OR LOWER(Uc.LastName) LIKE '%' + LOWER(@SearchText) + '%'
			OR LOWER(CONCAT(Uc.Name, ' ', Uc.LastName)) LIKE '%' + LOWER(@SearchText) + '%')
			AND ((@StartDate IS NULL AND @EndDate IS NULL) 
			OR (I.Date >= @StartDate 
			AND I.Date <= @EndDate))
            AND (@TransactionStatusId IS NULL OR I.TransactionStatusId = @TransactionStatusId);

            -- Request with pagination
            SELECT InterviewId
            ,Uc.Name + ' ' + Uc.LastName AS ConsultantName
            ,I.DurationMinutes
            ,I.Date
            ,I.CreationDate
            ,Ucreate.Name + ' ' + Ucreate.LastName AS CreatedBy
            ,I.LastUpdateDate
            ,Uupdate.Name + ' ' + Uupdate.LastName AS LastUpdatedBy
            ,TS.Name AS TransactionStatusName
            FROM INTERVIEWS I
            INNER JOIN TRANSACTION_STATUSES TS ON I.TransactionStatusId = TS.TransactionStatusId
            INNER JOIN CONSULTANT_DETAILS CD ON I.ConsultantId = CD.ConsultantId
            INNER JOIN Users Uc ON CD.UserId = Uc.Id
            INNER JOIN CONSULTANT_DETAILS CC ON I.ConsultantIdCreatedBy = CC.ConsultantId
            INNER JOIN Users Ucreate ON CC.UserId = Ucreate.Id
            LEFT JOIN CONSULTANT_DETAILS CU ON I.ConsultantIdLastUpdatedBy = CU.ConsultantId
            LEFT JOIN Users Uupdate ON CU.UserId = Uupdate.Id
            WHERE (@SearchText IS NULL 
            OR LOWER(Uc.Name) LIKE '%' + LOWER(@SearchText) + '%'
			OR LOWER(Uc.LastName) LIKE '%' + LOWER(@SearchText) + '%'
			OR LOWER(CONCAT(Uc.Name, ' ', Uc.LastName)) LIKE '%' + LOWER(@SearchText) + '%')
			AND ((@StartDate IS NULL AND @EndDate IS NULL) 
			OR (I.Date >= @StartDate 
			AND I.Date <= @EndDate))
            AND (@TransactionStatusId IS NULL OR I.TransactionStatusId = @TransactionStatusId)
			ORDER BY 
            CASE WHEN @FieldToOrder = 'ConsultantName' AND @DirectionOrder = 'ASC' THEN UC.Name END ASC,
            CASE WHEN @FieldToOrder = 'ConsultantName' AND @DirectionOrder = 'DESC' THEN UC.Name END DESC,
			CASE WHEN @FieldToOrder = 'DurationMinutes' AND @DirectionOrder = 'ASC' THEN I.DurationMinutes END ASC,
            CASE WHEN @FieldToOrder = 'DurationMinutes' AND @DirectionOrder = 'DESC' THEN I.DurationMinutes END DESC,
			CASE WHEN @FieldToOrder = 'Date' AND @DirectionOrder = 'ASC' THEN I.Date END ASC,
            CASE WHEN @FieldToOrder = 'Date' AND @DirectionOrder = 'DESC' THEN I.Date END DESC,
			CASE WHEN @FieldToOrder = 'CreationDate' AND @DirectionOrder = 'ASC' THEN I.CreationDate END ASC,
            CASE WHEN @FieldToOrder = 'CreationDate' AND @DirectionOrder = 'DESC' THEN I.CreationDate END DESC,
			CASE WHEN @FieldToOrder = 'LastUpdateDate' AND @DirectionOrder = 'ASC' THEN I.LastUpdateDate END ASC,
            CASE WHEN @FieldToOrder = 'LastUpdateDate' AND @DirectionOrder = 'DESC' THEN I.LastUpdateDate END DESC,
			CASE WHEN @FieldToOrder = 'CreatedBy' AND @DirectionOrder = 'ASC' THEN Ucreate.Name END ASC,
            CASE WHEN @FieldToOrder = 'CreatedBy' AND @DirectionOrder = 'DESC' THEN Ucreate.Name END DESC,
		    CASE WHEN @FieldToOrder = 'LastUpdatedBy' AND @DirectionOrder = 'ASC' THEN Uupdate.Name END ASC,
            CASE WHEN @FieldToOrder = 'LastUpdatedBy' AND @DirectionOrder = 'DESC' THEN Uupdate.Name END DESC,
            CASE WHEN @FieldToOrder = 'TransactionStatusName' AND @DirectionOrder = 'ASC' THEN TS.Name END ASC,
            CASE WHEN @FieldToOrder = 'TransactionStatusName' AND @DirectionOrder = 'DESC' THEN TS.Name END DESC,
            I.CreationDate DESC
            OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;
            END";

            // Delete stored procedure if exists
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS SP_INTERVIEWS_GetAllInterviewsWithFilters");

            // Create new stored Procedure
            migrationBuilder.Sql(sp);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // restart the stored procedure to the original version
            var spOriginal = @"CREATE PROCEDURE SP_INTERVIEWS_GetAllInterviewsWithFilters
            @SearchText NVARCHAR(255),
            @StartDate DATE,
            @EndDate DATE,
            @FieldToOrder NVARCHAR(255),
            @DirectionOrder NVARCHAR(255),
            @Skip INT,
            @Take INT,
            @TotalCount INT OUTPUT
            AS
            BEGIN
            -- Count total results
            SELECT @TotalCount = COUNT(*)
            FROM INTERVIEWS I
            INNER JOIN CONSULTANT_DETAILS CD ON I.ConsultantId = CD.ConsultantId
            INNER JOIN Users Uc ON CD.UserId = Uc.Id
            INNER JOIN CONSULTANT_DETAILS CC ON I.ConsultantIdCreatedBy = CC.ConsultantId
            INNER JOIN Users Ucreate ON CC.UserId = Ucreate.Id
            LEFT JOIN CONSULTANT_DETAILS CU ON I.ConsultantIdLastUpdatedBy = CU.ConsultantId
            LEFT JOIN Users Uupdate ON CU.UserId = Uupdate.Id
            WHERE (@SearchText IS NULL 
            OR LOWER(Uc.Name) LIKE '%' + LOWER(@SearchText) + '%'
			OR LOWER(Uc.LastName) LIKE '%' + LOWER(@SearchText) + '%'
			OR LOWER(CONCAT(Uc.Name, ' ', Uc.LastName)) LIKE '%' + LOWER(@SearchText) + '%')
			AND ((@StartDate IS NULL AND @EndDate IS NULL) 
			OR (I.Date >= @StartDate 
			AND I.Date <= @EndDate));

            -- Request with pagination
            SELECT InterviewId
            ,Uc.Name + ' ' + Uc.LastName AS ConsultantName
            ,I.DurationMinutes
            ,I.Date
            ,I.CreationDate
            ,Ucreate.Name + ' ' + Ucreate.LastName AS CreatedBy
            ,I.LastUpdateDate
            ,Uupdate.Name + ' ' + Uupdate.LastName AS LastUpdatedBy
            FROM INTERVIEWS I
            INNER JOIN CONSULTANT_DETAILS CD ON I.ConsultantId = CD.ConsultantId
            INNER JOIN Users Uc ON CD.UserId = Uc.Id
            INNER JOIN CONSULTANT_DETAILS CC ON I.ConsultantIdCreatedBy = CC.ConsultantId
            INNER JOIN Users Ucreate ON CC.UserId = Ucreate.Id
            LEFT JOIN CONSULTANT_DETAILS CU ON I.ConsultantIdLastUpdatedBy = CU.ConsultantId
            LEFT JOIN Users Uupdate ON CU.UserId = Uupdate.Id
            WHERE (@SearchText IS NULL 
            OR LOWER(Uc.Name) LIKE '%' + LOWER(@SearchText) + '%'
			OR LOWER(Uc.LastName) LIKE '%' + LOWER(@SearchText) + '%'
			OR LOWER(CONCAT(Uc.Name, ' ', Uc.LastName)) LIKE '%' + LOWER(@SearchText) + '%')
			AND ((@StartDate IS NULL AND @EndDate IS NULL) 
			OR (I.Date >= @StartDate 
			AND I.Date <= @EndDate))
			ORDER BY 
            CASE WHEN @FieldToOrder = 'ConsultantName' AND @DirectionOrder = 'ASC' THEN UC.Name END ASC,
            CASE WHEN @FieldToOrder = 'ConsultantName' AND @DirectionOrder = 'DESC' THEN UC.Name END DESC,
			CASE WHEN @FieldToOrder = 'DurationMinutes' AND @DirectionOrder = 'ASC' THEN I.DurationMinutes END ASC,
            CASE WHEN @FieldToOrder = 'DurationMinutes' AND @DirectionOrder = 'DESC' THEN I.DurationMinutes END DESC,
			CASE WHEN @FieldToOrder = 'Date' AND @DirectionOrder = 'ASC' THEN I.Date END ASC,
            CASE WHEN @FieldToOrder = 'Date' AND @DirectionOrder = 'DESC' THEN I.Date END DESC,
			CASE WHEN @FieldToOrder = 'CreationDate' AND @DirectionOrder = 'ASC' THEN I.CreationDate END ASC,
            CASE WHEN @FieldToOrder = 'CreationDate' AND @DirectionOrder = 'DESC' THEN I.CreationDate END DESC,
			CASE WHEN @FieldToOrder = 'LastUpdateDate' AND @DirectionOrder = 'ASC' THEN I.LastUpdateDate END ASC,
            CASE WHEN @FieldToOrder = 'LastUpdateDate' AND @DirectionOrder = 'DESC' THEN I.LastUpdateDate END DESC,
			CASE WHEN @FieldToOrder = 'CreatedBy' AND @DirectionOrder = 'ASC' THEN Ucreate.Name END ASC,
            CASE WHEN @FieldToOrder = 'CreatedBy' AND @DirectionOrder = 'DESC' THEN Ucreate.Name END DESC,
		    CASE WHEN @FieldToOrder = 'LastUpdatedBy' AND @DirectionOrder = 'ASC' THEN Uupdate.Name END ASC,
            CASE WHEN @FieldToOrder = 'LastUpdatedBy' AND @DirectionOrder = 'DESC' THEN Uupdate.Name END DESC,
            I.CreationDate DESC
            OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;
            END";

            migrationBuilder.Sql("DROP PROCEDURE IF SP_INTERVIEWS_GetAllInterviewsWithFilters");
            migrationBuilder.Sql(spOriginal);
        }
    }
}
