using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class addSP_PAYMENT_SHEETS_GetAllConsultantsToPayWithFilters : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sp = @"CREATE PROCEDURE SP_PAYMENT_SHEETS_GetAllConsultantsToPayWithFilters
            @SearchText NVARCHAR(255),
            @StartDate DATE,
            @EndDate DATE,
            @TransactionStatusId INT,
            @ProjectId INT,
            @PaymentPeriod INT,
            @FieldToOrder NVARCHAR(255),
            @DirectionOrder NVARCHAR(255),
            @Skip INT,
            @Take INT,
            @TotalCount INT OUTPUT
            AS
            BEGIN
            -- Count total results
            WITH ConsultantProjectActions AS (
        SELECT
        PCA.ProjectConsultantAssignedId,
        PCA.ProjectId,
        PCA.ConsultantId,
        PCAH.ActionDate,
        PCAH.Id,
        PCHA.Name AS ActionName,
        ROW_NUMBER() OVER (
            PARTITION BY PCA.ConsultantId, PCA.ProjectId, CONVERT(date, PCAH.ActionDate) 
            ORDER BY PCAH.ActionDate DESC, PCAH.Id DESC
        ) AS RowNum
        FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH
        INNER JOIN PROJECTS_CONSULTANTS_ASSIGNED PCA ON PCAH.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
        INNER JOIN PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS PCHA ON PCAH.ActionId = PCHA.ActionId
        WHERE PCAH.ActionDate <= @EndDate
        ),
        ConsultantStatus AS (
        SELECT
        CPA.ProjectId,
        CPA.ConsultantId,
        CPA.ActionDate,
        CPA.ActionName
        FROM ConsultantProjectActions CPA
        WHERE CPA.RowNum = 1
        ),
        ActiveConsultants AS (
        SELECT
            CS.ProjectId,
            CS.ConsultantId
        FROM ConsultantStatus CS
        WHERE (CS.ActionName = 'Consultant Activated' OR CS.ActionName = 'Consultant Assigned First Time') 
        AND CS.ActionDate <= @EndDate
        ),
        DeactivatedConsultants AS (
        SELECT
            CS.ProjectId,
            CS.ConsultantId
        FROM ConsultantStatus CS
        WHERE CS.ActionName = 'Consultant Deactivated' AND CS.ActionDate < @StartDate
        ),
        ConsultantsToReport AS (
            SELECT
                AC.ProjectId,
                AC.ConsultantId
            FROM ActiveConsultants AC
            WHERE NOT EXISTS (
                SELECT 1
                FROM DeactivatedConsultants DC
                WHERE DC.ConsultantId = AC.ConsultantId AND DC.ProjectId = AC.ProjectId
            )
            UNION
            SELECT
                CS.ProjectId,
                CS.ConsultantId
            FROM ConsultantStatus CS
            WHERE CS.ActionName = 'Consultant Deactivated' AND CS.ActionDate BETWEEN @StartDate AND @EndDate
        ),
        SubmissionDetails AS (
        SELECT
            RMS.ProjectId,
            RMS.ConsultantId,
            RMS.SubmissionId,
            RMS.SubmissionDate,
            RMS.LastSubmissionDate,
            TS.Name AS TransactionStatusName,
	    	RMS.TransactionStatusId
        FROM REPORTING_MY_TIME_MOVEMENTS_SUBMISSIONS RMS
        INNER JOIN TRANSACTION_STATUSES TS ON RMS.TransactionStatusId = TS.TransactionStatusId
        WHERE CONVERT(date, RMS.StartPeriodDate) = CONVERT(date, @StartDate)
        AND CONVERT(date, RMS.EndPeriodDate) = CONVERT(date, @EndDate)
        )
        SELECT 
            @TotalCount = COUNT(*)
        FROM ConsultantsToReport CTR
        JOIN CONSULTANT_DETAILS CD ON CTR.ConsultantId = CD.ConsultantId
        JOIN USERS U ON CD.UserId = U.Id
        JOIN PROJECTS P ON CTR.ProjectId = P.ProjectId
        LEFT JOIN SubmissionDetails SD ON CTR.ProjectId = SD.ProjectId AND CTR.ConsultantId = SD.ConsultantId
        WHERE (@PaymentPeriod IS NULL OR CD.PaymentPeriod = @PaymentPeriod)
        AND (@TransactionStatusId IS NULL OR SD.TransactionStatusId = @TransactionStatusId)
        AND (@ProjectId IS NULL OR P.ProjectId = @ProjectId)
        AND (@SearchText IS NULL 
            OR LOWER(U.Name) LIKE '%' + LOWER(@SearchText) + '%'
			OR LOWER(U.LastName) LIKE '%' + LOWER(@SearchText) + '%'
			OR LOWER(CONCAT(U.Name, ' ', U.LastName)) LIKE '%' + LOWER(@SearchText) + '%');

            -- Request with pagination
            WITH ConsultantProjectActions AS (
             SELECT
                 PCA.ProjectConsultantAssignedId,
                 PCA.ProjectId,
                 PCA.ConsultantId,
                 PCAH.ActionDate,
                 PCAH.Id,
                 PCHA.Name AS ActionName,
                 ROW_NUMBER() OVER (
                     PARTITION BY PCA.ConsultantId, PCA.ProjectId, CONVERT(date, PCAH.ActionDate) 
                     ORDER BY PCAH.ActionDate DESC, PCAH.Id DESC
                 ) AS RowNum
             FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH
             INNER JOIN PROJECTS_CONSULTANTS_ASSIGNED PCA ON PCAH.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
             INNER JOIN PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS PCHA ON PCAH.ActionId = PCHA.ActionId
             WHERE PCAH.ActionDate <= @EndDate
         ),
         ConsultantStatus AS (
             SELECT
                 CPA.ProjectId,
                 CPA.ConsultantId,
                 CPA.ActionDate,
                 CPA.ActionName
             FROM ConsultantProjectActions CPA
             WHERE CPA.RowNum = 1
         ),
         ActiveConsultants AS (
             SELECT
                 CS.ProjectId,
                 CS.ConsultantId
             FROM ConsultantStatus CS
             WHERE (CS.ActionName = 'Consultant Activated' OR CS.ActionName = 'Consultant Assigned First Time') 
                 AND CS.ActionDate <= @EndDate
         ),
         DeactivatedConsultants AS (
             SELECT
                 CS.ProjectId,
                 CS.ConsultantId
             FROM ConsultantStatus CS
             WHERE CS.ActionName = 'Consultant Deactivated' AND CS.ActionDate < @StartDate
         ),
         ConsultantsToReport AS (
             SELECT
                 AC.ProjectId,
                 AC.ConsultantId
             FROM ActiveConsultants AC
             WHERE NOT EXISTS (
                 SELECT 1
                 FROM DeactivatedConsultants DC
                 WHERE DC.ConsultantId = AC.ConsultantId AND DC.ProjectId = AC.ProjectId
             )
             UNION
             SELECT
                 CS.ProjectId,
                 CS.ConsultantId
             FROM ConsultantStatus CS
             WHERE CS.ActionName = 'Consultant Deactivated' AND CS.ActionDate BETWEEN @StartDate AND @EndDate
         ),
         SubmissionDetails AS (
             SELECT
                 RMS.ProjectId,
                 RMS.ConsultantId,
                 RMS.SubmissionId,
                 RMS.SubmissionDate,
                 RMS.LastSubmissionDate,
                 TS.Name AS TransactionStatusName,
         		RMS.TransactionStatusId
             FROM REPORTING_MY_TIME_MOVEMENTS_SUBMISSIONS RMS
             INNER JOIN TRANSACTION_STATUSES TS ON RMS.TransactionStatusId = TS.TransactionStatusId
             WHERE CONVERT(date, RMS.StartPeriodDate) = CONVERT(date, @StartDate)
                 AND CONVERT(date, RMS.EndPeriodDate) = CONVERT(date, @EndDate)
         )
         SELECT 
             CONCAT(U.Name, ' ', U.LastName) AS ConsultantName,
             CTR.ProjectId,
         	P.Name AS ProjectName,
             SD.SubmissionId,
             SD.TransactionStatusName,
             SD.SubmissionDate,
             SD.LastSubmissionDate
         FROM ConsultantsToReport CTR
         JOIN CONSULTANT_DETAILS CD ON CTR.ConsultantId = CD.ConsultantId
         JOIN USERS U ON CD.UserId = U.Id
         JOIN PROJECTS P ON CTR.ProjectId = P.ProjectId
         LEFT JOIN SubmissionDetails SD ON CTR.ProjectId = SD.ProjectId AND CTR.ConsultantId = SD.ConsultantId
         WHERE (@PaymentPeriod IS NULL OR CD.PaymentPeriod = @PaymentPeriod)
         AND (@TransactionStatusId IS NULL OR SD.TransactionStatusId = @TransactionStatusId)
         AND (@ProjectId IS NULL OR P.ProjectId = @ProjectId)
         AND (@SearchText IS NULL 
                     OR LOWER(U.Name) LIKE '%' + LOWER(@SearchText) + '%'
         			OR LOWER(U.LastName) LIKE '%' + LOWER(@SearchText) + '%'
         			OR LOWER(CONCAT(U.Name, ' ', U.LastName)) LIKE '%' + LOWER(@SearchText) + '%')
         
         ORDER BY 
         CASE WHEN @FieldToOrder = 'ConsultantName' AND @DirectionOrder = 'ASC' THEN CONCAT(U.Name, ' ', U.LastName) END ASC,
         CASE WHEN @FieldToOrder = 'ConsultantName' AND @DirectionOrder = 'DESC' THEN CONCAT(U.Name, ' ', U.LastName) END DESC,
         CASE WHEN @FieldToOrder = 'ProjectName' AND @DirectionOrder = 'ASC' THEN P.Name END ASC,
         CASE WHEN @FieldToOrder = 'ProjectName' AND @DirectionOrder = 'DESC' THEN P.Name END DESC,
         CASE WHEN @FieldToOrder = 'TransactionStatusName' AND @DirectionOrder = 'ASC' THEN SD.TransactionStatusName END ASC,
         CASE WHEN @FieldToOrder = 'TransactionStatusName' AND @DirectionOrder = 'DESC' THEN SD.TransactionStatusName END DESC,
         CASE WHEN @FieldToOrder = 'SubmissionDate' AND @DirectionOrder = 'ASC' THEN SD.SubmissionDate END ASC,
         CASE WHEN @FieldToOrder = 'SubmissionDate' AND @DirectionOrder = 'DESC' THEN SD.SubmissionDate END DESC,
         CASE WHEN @FieldToOrder = 'LastSubmissionDate' AND @DirectionOrder = 'ASC' THEN SD.LastSubmissionDate END ASC,
         CASE WHEN @FieldToOrder = 'LastSubmissionDate' AND @DirectionOrder = 'DESC' THEN SD.LastSubmissionDate END DESC,
         CONCAT(U.Name, ' ', U.LastName)
         OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;
            END";
            migrationBuilder.Sql(sp);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS SP_PAYMENT_SHEETS_GetAllConsultantsToPayWithFilters");
        }
    }
}
