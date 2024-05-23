using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class fourteenUpdateSP_PAYMENT_SHEETS_GetAllConsultantsToPayWithFilters : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //UPDATE STORED PROCEDURE
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
                SET NOCOUNT ON;
            
                WITH SubmissionsCounts AS (
                    SELECT 
                        ConsultantId,
                        ProjectId,
                        COUNT(*) AS NumSubmissions
                    FROM 
                        REPORTING_MY_TIME_MOVEMENTS_SUBMISSIONS SCRMTS
                        INNER JOIN TRANSACTION_STATUSES SCTS ON SCRMTS.TransactionStatusId = SCTS.TransactionStatusId
                    WHERE SCTS.Name = 'Approved'
                    AND SCRMTS.StartPeriodDate = @StartDate
                    AND SCRMTS.EndPeriodDate = @EndDate
                    GROUP BY ConsultantId, ProjectId
                ),
                LatestSalaryUpdates AS (
                    SELECT 
                        PCAH.ProjectConsultantAssignedId,
                        MAX(CASE 
                            WHEN HA.Name = 'Hourly Salary updated' THEN PCAH.ActionDate
                            ELSE NULL
                        END) AS LastHourlySalaryUpdate,
                        MAX(CASE 
                            WHEN HA.Name = 'Monthly Salary updated' THEN PCAH.ActionDate
                            ELSE NULL
                        END) AS LastMonthlySalaryUpdate,
                        MAX(CASE 
                            WHEN HA.Name = 'Consultant pricing method updated (Hourly)' THEN PCAH.ActionDate
                            ELSE NULL
                        END) AS LastHourlyPricingUpdate,
                        MAX(CASE 
                            WHEN HA.Name = 'Consultant pricing method updated (Monthly)' THEN PCAH.ActionDate
                            ELSE NULL
                        END) AS LastMonthlyPricingUpdate
                    FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH
                    JOIN PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS HA ON PCAH.ActionId = HA.ActionId
                    WHERE PCAH.ActionDate <= @EndDate
                    GROUP BY PCAH.ProjectConsultantAssignedId
                ),
                ValidConsultants AS (
                    SELECT
                        PCA.ProjectConsultantAssignedId,
                        PCA.ConsultantId,
                        PCA.ProjectId
                    FROM PROJECTS_CONSULTANTS_ASSIGNED PCA
                    LEFT JOIN LatestSalaryUpdates LSU ON PCA.ProjectConsultantAssignedId = LSU.ProjectConsultantAssignedId
                    WHERE 
                        -- Check if the latest salary updates are greater than zero
                        (
                            NOT EXISTS (
                                SELECT 1
                                FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH
                                INNER JOIN PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS HA ON PCAH.ActionId = HA.ActionId
                                WHERE PCAH.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
                                AND PCAH.ActionDate <= @EndDate
                                AND (
                                    (HA.Name = 'Consultant pricing method updated (Hourly)' AND
                                    EXISTS (
                                        SELECT 1
                                        FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH2
                                        INNER JOIN PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS HA2 ON PCAH2.ActionId = HA2.ActionId
                                        WHERE PCAH2.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
                                        AND PCAH2.ActionDate <= @EndDate
                                        AND HA2.Name = 'Hourly Salary updated'
                                        AND PCAH2.NewValue = 0
                                    )) OR
                                    (HA.Name = 'Consultant pricing method updated (Monthly)' AND
                                    EXISTS (
                                        SELECT 1
                                        FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH2
                                        INNER JOIN PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS HA2 ON PCAH2.ActionId = HA2.ActionId
                                        WHERE PCAH2.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
                                        AND PCAH2.ActionDate <= @EndDate
                                        AND HA2.Name = 'Monthly Salary updated'
                                        AND PCAH2.NewValue = 0
                                    ))
                                )
                            ) OR
                            -- If no pricing method updates, validate using the latest salary updates directly
                            (
                                (LSU.LastHourlyPricingUpdate IS NULL AND LSU.LastMonthlyPricingUpdate IS NULL)
                                AND (
                                    NOT EXISTS (
                                        SELECT 1
                                        FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH
                                        INNER JOIN PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS HA ON PCAH.ActionId = HA.ActionId
                                        WHERE PCAH.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
                                        AND PCAH.ActionDate <= @EndDate
                                        AND HA.Name = 'Hourly Salary updated'
                                        AND PCAH.NewValue = 0
                                    ) AND
                                    NOT EXISTS (
                                        SELECT 1
                                        FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH
                                        INNER JOIN PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS HA ON PCAH.ActionId = HA.ActionId
                                        WHERE PCAH.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
                                        AND PCAH.ActionDate <= @EndDate
                                        AND HA.Name = 'Monthly Salary updated'
                                        AND PCAH.NewValue = 0
                                    )
                                )
                            ) OR
                            -- Directly check for zero salaries without updates
                            (PCA.HourlySalary > 0 OR PCA.MonthlySalary > 0)
                        )
                ),
                NumApprovedSubmissionsConsistent AS (
                    SELECT
                        ConsultantId,
                        SUM(CASE 
                            WHEN COALESCE(NumSubmissions, 0) > 0 THEN 1
                            WHEN AccessToTrackingTool = 0 THEN 1
                            ELSE 0
                        END) AS NumApprovedSubmissions
                    FROM (
                        SELECT
                            CD.ConsultantId,
                            P.ProjectId,
                            COALESCE(SC.NumSubmissions, 0) AS NumSubmissions,
                            PCA.AccessToTrackingTool
                        FROM PROJECTS_CONSULTANTS_ASSIGNED PCA
                        JOIN CONSULTANT_DETAILS CD ON PCA.ConsultantId = CD.ConsultantId
                        JOIN PROJECTS P ON PCA.ProjectId = P.ProjectId
                        LEFT JOIN SubmissionsCounts SC ON CD.ConsultantId = SC.ConsultantId AND P.ProjectId = SC.ProjectId
                        LEFT JOIN LatestSalaryUpdates LSU ON PCA.ProjectConsultantAssignedId = LSU.ProjectConsultantAssignedId
                        WHERE 
                            PCA.ProjectConsultantAssignedId IN (SELECT ProjectConsultantAssignedId FROM ValidConsultants)
                    ) AS SubQuery
                    GROUP BY ConsultantId
                ),
                ActiveProjects AS (
                    SELECT
                        PCA.ConsultantId,
                        COUNT(DISTINCT P.ProjectId) AS NumProjectsIsActive
                    FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH
                    JOIN PROJECTS_CONSULTANTS_ASSIGNED PCA ON PCAH.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
                    JOIN PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS HA ON PCAH.ActionId = HA.ActionId
                    JOIN PROJECTS P ON PCA.ProjectId = P.ProjectId
                    LEFT JOIN LatestSalaryUpdates LSU ON PCA.ProjectConsultantAssignedId = LSU.ProjectConsultantAssignedId
                    WHERE
                        PCAH.ActionDate <= @EndDate AND (
                            EXISTS (
                                SELECT 1
                                FROM (
                                    SELECT TOP(1)
                                        CASE
                                            WHEN HA.Name = 'Consultant Activated' THEN 'Active'
                                            WHEN HA.Name = 'Consultant Deactivated' AND PCAH.ActionDate = @StartDate THEN 'Active'
                                            WHEN HA.Name = 'Consultant Assigned First Time' THEN 'Assigned First Time'
                                            ELSE 'No actions'
                                        END AS ActionOutcome
                                    FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH
                                    INNER JOIN PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS HA ON PCAH.ActionId = HA.ActionId
                                    WHERE PCAH.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
                                    AND PCAH.ActionDate <= @EndDate
                                    ORDER BY PCAH.ActionDate DESC, PCAH.Id DESC
                                ) SubQuery
                                WHERE SubQuery.ActionOutcome = 'Active' OR SubQuery.ActionOutcome = 'Assigned First Time'
                            ) OR 
                            EXISTS (
                                SELECT 1
                                FROM (
                                    SELECT TOP(1)
                                        CASE
                                            WHEN HA.Name = 'Consultant Activated' THEN 'Active'
                                            ELSE 'No actions'
                                        END AS ActionOutcome
                                    FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH
                                    INNER JOIN PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS HA ON PCAH.ActionId = HA.ActionId
                                    WHERE PCAH.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
                                    AND HA.Name = 'Consultant Activated'
                                    AND PCAH.ActionDate >= @StartDate 
                                    AND PCAH.ActionDate <= @EndDate
                                    ORDER BY PCAH.ActionDate DESC, PCAH.Id DESC
                                ) SubQuery
                                WHERE SubQuery.ActionOutcome = 'Active'
                            ) OR 
                            NOT EXISTS (
                                SELECT 1
                                FROM (
                                    SELECT TOP(1)
                                        CASE
                                            WHEN HA.Name = 'Consultant Deactivated' THEN 'Inactive'
                                            ELSE 'No actions'
                                        END AS ActionOutcome
                                    FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH
                                    INNER JOIN PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS HA ON PCAH.ActionId = HA.ActionId
                                    WHERE PCAH.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
                                    AND HA.Name = 'Consultant Deactivated'
                                    AND PCAH.ActionDate < @StartDate 
                                    ORDER BY PCAH.ActionDate DESC, PCAH.Id DESC
                                ) SubQuery
                                WHERE SubQuery.ActionOutcome = 'Inactive'
                            )
                        )
                        AND PCA.ProjectConsultantAssignedId IN (SELECT ProjectConsultantAssignedId FROM ValidConsultants)
                    GROUP BY PCA.ConsultantId
                ),
                FilteredConsultants AS (
                    SELECT
                        AP.ConsultantId,
                        PCA.ProjectId,
                        AP.NumProjectsIsActive
                    FROM ActiveProjects AP
                    JOIN PROJECTS_CONSULTANTS_ASSIGNED PCA ON AP.ConsultantId = PCA.ConsultantId
                    WHERE PCA.ProjectConsultantAssignedId IN (SELECT ProjectConsultantAssignedId FROM ValidConsultants)
                )
            
                -- Count total results
                SELECT @TotalCount = COUNT(*)
                FROM (
                    SELECT
                        CONCAT(U.Name, ' ', U.LastName) AS ConsultantName,
                        CD.ConsultantId,
                        P.ProjectId,
                        P.Name AS ProjectName,
                        CASE 
                            WHEN PCA.AccessToTrackingTool = 0 AND RS.SubmissionId IS NULL THEN NULL 
                            ELSE RS.SubmissionId 
                        END AS SubmissionId,
                        CASE 
                            WHEN PCA.AccessToTrackingTool = 0 AND RS.TransactionStatusId IS NULL THEN 'Approved' 
                            ELSE TS.Name 
                        END AS TransactionStatusName,
                        CASE 
                            WHEN PCA.AccessToTrackingTool = 0 AND RS.SubmissionDate IS NULL THEN NULL 
                            ELSE RS.SubmissionDate 
                        END AS SubmissionDate,
                        CASE 
                            WHEN PCA.AccessToTrackingTool = 0 AND RS.LastSubmissionDate IS NULL THEN NULL 
                            ELSE RS.LastSubmissionDate 
                        END AS LastSubmissionDate,
                        NAS.NumApprovedSubmissions,
                        FAP.NumProjectsIsActive,
                        PCA.AccessToTrackingTool
                    FROM FilteredConsultants FAP
                    JOIN PROJECTS_CONSULTANTS_ASSIGNED PCA ON FAP.ConsultantId = PCA.ConsultantId AND FAP.ProjectId = PCA.ProjectId
                    JOIN PROJECTS P ON PCA.ProjectId = P.ProjectId
                    JOIN CONSULTANT_DETAILS CD ON PCA.ConsultantId = CD.ConsultantId
                    JOIN Users U ON CD.UserId = U.Id
                    LEFT JOIN REPORTING_MY_TIME_MOVEMENTS_SUBMISSIONS RS ON PCA.ConsultantId = RS.ConsultantId
                    AND P.ProjectId = RS.ProjectId AND RS.StartPeriodDate = @StartDate
                    AND RS.EndPeriodDate = @EndDate
                    LEFT JOIN TRANSACTION_STATUSES TS ON RS.TransactionStatusId = TS.TransactionStatusId
                    LEFT JOIN SubmissionsCounts SC ON CD.ConsultantId = SC.ConsultantId AND P.ProjectId = SC.ProjectId
                    LEFT JOIN NumApprovedSubmissionsConsistent NAS ON CD.ConsultantId = NAS.ConsultantId
                    WHERE 
                        (@ProjectId IS NULL OR P.ProjectId = @ProjectId) AND
                        (@TransactionStatusId IS NULL OR (TS.TransactionStatusId = @TransactionStatusId AND TS.TransactionStatusId IS NOT NULL)) AND
                        (@SearchText IS NULL 
                            OR LOWER(U.Name) LIKE '%' + LOWER(@SearchText) + '%' 
                            OR LOWER(U.LastName) LIKE '%' + LOWER(@SearchText) + '%' 
                            OR LOWER(CONCAT(U.Name, ' ', U.LastName)) LIKE '%' + LOWER(@SearchText) + '%') AND
                        (@PaymentPeriod IS NULL OR CD.PaymentPeriod = @PaymentPeriod)
                    GROUP BY CONCAT(U.Name, ' ', U.LastName), CD.ConsultantId, P.ProjectId, P.Name, RS.SubmissionId, RS.TransactionStatusId, TS.Name, RS.SubmissionDate, RS.LastSubmissionDate, NAS.NumApprovedSubmissions, FAP.NumProjectsIsActive, PCA.AccessToTrackingTool
                ) AS TotalRegisters;
                
                -- Request with pagination
                SELECT
                    CONCAT(U.Name, ' ', U.LastName) AS ConsultantName,
                    CD.ConsultantId,
                    P.ProjectId,
                    P.Name AS ProjectName,
                    CASE 
                        WHEN PCA.AccessToTrackingTool = 0 AND RS.SubmissionId IS NULL THEN NULL 
                        ELSE RS.SubmissionId 
                    END AS SubmissionId,
                    CASE 
                        WHEN PCA.AccessToTrackingTool = 0 AND RS.TransactionStatusId IS NULL THEN 'Approved' 
                        ELSE TS.Name 
                    END AS TransactionStatusName,
                    CASE 
                        WHEN PCA.AccessToTrackingTool = 0 AND RS.SubmissionDate IS NULL THEN NULL 
                        ELSE RS.SubmissionDate 
                    END AS SubmissionDate,
                    CASE 
                        WHEN PCA.AccessToTrackingTool = 0 AND RS.LastSubmissionDate IS NULL THEN NULL 
                        ELSE RS.LastSubmissionDate 
                    END AS LastSubmissionDate,
                    NAS.NumApprovedSubmissions,
                    FAP.NumProjectsIsActive,
                    PCA.AccessToTrackingTool
                FROM FilteredConsultants FAP
                JOIN PROJECTS_CONSULTANTS_ASSIGNED PCA ON FAP.ConsultantId = PCA.ConsultantId AND FAP.ProjectId = PCA.ProjectId
                JOIN PROJECTS P ON PCA.ProjectId = P.ProjectId
                JOIN CONSULTANT_DETAILS CD ON PCA.ConsultantId = CD.ConsultantId
                JOIN Users U ON CD.UserId = U.Id
                LEFT JOIN REPORTING_MY_TIME_MOVEMENTS_SUBMISSIONS RS ON PCA.ConsultantId = RS.ConsultantId
                AND P.ProjectId = RS.ProjectId AND RS.StartPeriodDate = @StartDate
                AND RS.EndPeriodDate = @EndDate
                LEFT JOIN TRANSACTION_STATUSES TS ON RS.TransactionStatusId = TS.TransactionStatusId
                LEFT JOIN SubmissionsCounts SC ON CD.ConsultantId = SC.ConsultantId AND P.ProjectId = SC.ProjectId
                LEFT JOIN NumApprovedSubmissionsConsistent NAS ON CD.ConsultantId = NAS.ConsultantId
                WHERE 
                    (@ProjectId IS NULL OR P.ProjectId = @ProjectId) AND
                    (@TransactionStatusId IS NULL OR (TS.TransactionStatusId = @TransactionStatusId AND TS.TransactionStatusId IS NOT NULL)) AND
                    (@SearchText IS NULL 
                        OR LOWER(U.Name) LIKE '%' + LOWER(@SearchText) + '%' 
                        OR LOWER(U.LastName) LIKE '%' + LOWER(@SearchText) + '%' 
                        OR LOWER(CONCAT(U.Name, ' ', U.LastName)) LIKE '%' + LOWER(@SearchText) + '%') AND
                    (@PaymentPeriod IS NULL OR CD.PaymentPeriod = @PaymentPeriod)
                GROUP BY CONCAT(U.Name, ' ', U.LastName), CD.ConsultantId, P.ProjectId, P.Name, RS.SubmissionId, RS.TransactionStatusId, TS.Name, RS.SubmissionDate, RS.LastSubmissionDate, NAS.NumApprovedSubmissions, FAP.NumProjectsIsActive, PCA.AccessToTrackingTool
                ORDER BY 
                    CASE WHEN @FieldToOrder = 'ConsultantName' AND @DirectionOrder = 'ASC' THEN CONCAT(U.Name, ' ', U.LastName) END ASC,
                    CASE WHEN @FieldToOrder = 'ConsultantName' AND @DirectionOrder = 'DESC' THEN CONCAT(U.Name, ' ', U.LastName) END DESC,
                    CASE WHEN @FieldToOrder = 'ProjectName' AND @DirectionOrder = 'ASC' THEN P.Name END ASC,
                    CASE WHEN @FieldToOrder = 'ProjectName' AND @DirectionOrder = 'DESC' THEN P.Name END DESC,
                    CASE WHEN @FieldToOrder = 'TransactionStatusName' AND @DirectionOrder = 'ASC' THEN TS.Name END ASC,
                    CASE WHEN @FieldToOrder = 'TransactionStatusName' AND @DirectionOrder = 'DESC' THEN TS.Name END DESC,
                    CASE WHEN @FieldToOrder = 'SubmissionDate' AND @DirectionOrder = 'ASC' THEN RS.SubmissionDate END ASC,
                    CASE WHEN @FieldToOrder = 'SubmissionDate' AND @DirectionOrder = 'DESC' THEN RS.SubmissionDate END DESC,
                    CASE WHEN @FieldToOrder = 'LastSubmissionDate' AND @DirectionOrder = 'ASC' THEN RS.LastSubmissionDate END ASC,
                    CASE WHEN @FieldToOrder = 'LastSubmissionDate' AND @DirectionOrder = 'DESC' THEN RS.LastSubmissionDate END DESC,
                    CONCAT(U.Name, ' ', U.LastName)
                OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;
            END";

            // Delete stored procedure if exists
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS SP_PAYMENT_SHEETS_GetAllConsultantsToPayWithFilters");

            // Create new stored Procedure
            migrationBuilder.Sql(sp);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // restart the stored procedure to the original version
            var spOriginal = @"CREATE PROCEDURE SP_PAYMENT_SHEETS_GetAllConsultantsToPayWithFilters
                @SearchText NVARCHAR(255) = NULL,
                @StartDate DATE,
                @EndDate DATE,
                @TransactionStatusId INT = NULL,
                @ProjectId INT = NULL,
                @PaymentPeriod INT = NULL,
                @FieldToOrder NVARCHAR(255) = 'ProjectName',
                @DirectionOrder NVARCHAR(255) = 'ASC',
                @Skip INT = 0,
                @Take INT = 100,
                @TotalCount INT OUTPUT
            AS
            BEGIN
                SET NOCOUNT ON;
            
                WITH SubmissionsCounts AS (
                    SELECT 
                        ConsultantId,
                        ProjectId,
                        COUNT(*) AS NumSubmissions
                    FROM 
                        REPORTING_MY_TIME_MOVEMENTS_SUBMISSIONS SCRMTS
                        INNER JOIN TRANSACTION_STATUSES SCTS ON SCRMTS.TransactionStatusId = SCTS.TransactionStatusId
                    WHERE SCTS.Name = 'Approved'
                    AND SCRMTS.StartPeriodDate = @StartDate
                    AND SCRMTS.EndPeriodDate = @EndDate
                    GROUP BY ConsultantId, ProjectId
                ),
                LatestSalaryUpdates AS (
                    SELECT 
                        PCAH.ProjectConsultantAssignedId,
                        MAX(CASE 
                            WHEN HA.Name = 'Hourly Salary updated' THEN PCAH.ActionDate
                            ELSE NULL
                        END) AS LastHourlySalaryUpdate,
                        MAX(CASE 
                            WHEN HA.Name = 'Monthly Salary updated' THEN PCAH.ActionDate
                            ELSE NULL
                        END) AS LastMonthlySalaryUpdate,
                        MAX(CASE 
                            WHEN HA.Name = 'Consultant pricing method updated (Hourly)' THEN PCAH.ActionDate
                            ELSE NULL
                        END) AS LastHourlyPricingUpdate,
                        MAX(CASE 
                            WHEN HA.Name = 'Consultant pricing method updated (Monthly)' THEN PCAH.ActionDate
                            ELSE NULL
                        END) AS LastMonthlyPricingUpdate
                    FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH
                    JOIN PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS HA ON PCAH.ActionId = HA.ActionId
                    WHERE PCAH.ActionDate <= @EndDate
                    GROUP BY PCAH.ProjectConsultantAssignedId
                ),
                ValidConsultants AS (
                    SELECT
                        PCA.ProjectConsultantAssignedId,
                        PCA.ConsultantId,
                        PCA.ProjectId
                    FROM PROJECTS_CONSULTANTS_ASSIGNED PCA
                    LEFT JOIN LatestSalaryUpdates LSU ON PCA.ProjectConsultantAssignedId = LSU.ProjectConsultantAssignedId
                    WHERE 
                        -- Check if the latest salary updates are greater than zero
                        (
                            NOT EXISTS (
                                SELECT 1
                                FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH
                                INNER JOIN PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS HA ON PCAH.ActionId = HA.ActionId
                                WHERE PCAH.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
                                AND PCAH.ActionDate <= @EndDate
                                AND (
                                    (HA.Name = 'Consultant pricing method updated (Hourly)' AND
                                    EXISTS (
                                        SELECT 1
                                        FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH2
                                        INNER JOIN PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS HA2 ON PCAH2.ActionId = HA2.ActionId
                                        WHERE PCAH2.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
                                        AND PCAH2.ActionDate <= @EndDate
                                        AND HA2.Name = 'Hourly Salary updated'
                                        AND PCAH2.NewValue = 0
                                    )) OR
                                    (HA.Name = 'Consultant pricing method updated (Monthly)' AND
                                    EXISTS (
                                        SELECT 1
                                        FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH2
                                        INNER JOIN PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS HA2 ON PCAH2.ActionId = HA2.ActionId
                                        WHERE PCAH2.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
                                        AND PCAH2.ActionDate <= @EndDate
                                        AND HA2.Name = 'Monthly Salary updated'
                                        AND PCAH2.NewValue = 0
                                    ))
                                )
                            ) OR
                            -- If no pricing method updates, validate using the latest salary updates directly
                            (
                                (LSU.LastHourlyPricingUpdate IS NULL AND LSU.LastMonthlyPricingUpdate IS NULL)
                                AND (
                                    NOT EXISTS (
                                        SELECT 1
                                        FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH
                                        INNER JOIN PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS HA ON PCAH.ActionId = HA.ActionId
                                        WHERE PCAH.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
                                        AND PCAH.ActionDate <= @EndDate
                                        AND HA.Name = 'Hourly Salary updated'
                                        AND PCAH.NewValue = 0
                                    ) AND
                                    NOT EXISTS (
                                        SELECT 1
                                        FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH
                                        INNER JOIN PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS HA ON PCAH.ActionId = HA.ActionId
                                        WHERE PCAH.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
                                        AND PCAH.ActionDate <= @EndDate
                                        AND HA.Name = 'Monthly Salary updated'
                                        AND PCAH.NewValue = 0
                                    )
                                )
                            ) OR
                            -- Directly check for zero salaries without updates
                            (PCA.HourlySalary > 0 OR PCA.MonthlySalary > 0)
                        )
                ),
                NumApprovedSubmissionsConsistent AS (
                    SELECT
                        ConsultantId,
                        SUM(CASE 
                            WHEN COALESCE(NumSubmissions, 0) > 0 THEN 1
                            WHEN AccessToTrackingTool = 0 THEN 1
                            ELSE 0
                        END) AS NumApprovedSubmissions
                    FROM (
                        SELECT
                            CD.ConsultantId,
                            P.ProjectId,
                            COALESCE(SC.NumSubmissions, 0) AS NumSubmissions,
                            PCA.AccessToTrackingTool
                        FROM PROJECTS_CONSULTANTS_ASSIGNED PCA
                        JOIN CONSULTANT_DETAILS CD ON PCA.ConsultantId = CD.ConsultantId
                        JOIN PROJECTS P ON PCA.ProjectId = P.ProjectId
                        LEFT JOIN SubmissionsCounts SC ON CD.ConsultantId = SC.ConsultantId AND P.ProjectId = SC.ProjectId
                        LEFT JOIN LatestSalaryUpdates LSU ON PCA.ProjectConsultantAssignedId = LSU.ProjectConsultantAssignedId
                        WHERE 
                            PCA.ProjectConsultantAssignedId IN (SELECT ProjectConsultantAssignedId FROM ValidConsultants)
                    ) AS SubQuery
                    GROUP BY ConsultantId
                ),
                ActiveProjects AS (
                    SELECT
                        PCA.ConsultantId,
                        COUNT(DISTINCT P.ProjectId) AS NumProjectsIsActive
                    FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH
                    JOIN PROJECTS_CONSULTANTS_ASSIGNED PCA ON PCAH.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
                    JOIN PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS HA ON PCAH.ActionId = HA.ActionId
                    JOIN PROJECTS P ON PCA.ProjectId = P.ProjectId
                    LEFT JOIN LatestSalaryUpdates LSU ON PCA.ProjectConsultantAssignedId = LSU.ProjectConsultantAssignedId
                    WHERE
                        PCAH.ActionDate <= @EndDate AND (
                            EXISTS (
                                SELECT 1
                                FROM (
                                    SELECT TOP(1)
                                        CASE
                                            WHEN HA.Name = 'Consultant Activated' THEN 'Active'
                                            WHEN HA.Name = 'Consultant Deactivated' AND PCAH.ActionDate = @StartDate THEN 'Active'
                                            WHEN HA.Name = 'Consultant Assigned First Time' THEN 'Assigned First Time'
                                            ELSE 'No actions'
                                        END AS ActionOutcome
                                    FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH
                                    INNER JOIN PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS HA ON PCAH.ActionId = HA.ActionId
                                    WHERE PCAH.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
                                    AND PCAH.ActionDate <= @EndDate
                                    ORDER BY PCAH.ActionDate DESC, PCAH.Id DESC
                                ) SubQuery
                                WHERE SubQuery.ActionOutcome = 'Active' OR SubQuery.ActionOutcome = 'Assigned First Time'
                            ) OR 
                            EXISTS (
                                SELECT 1
                                FROM (
                                    SELECT TOP(1)
                                        CASE
                                            WHEN HA.Name = 'Consultant Activated' THEN 'Active'
                                            ELSE 'No actions'
                                        END AS ActionOutcome
                                    FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH
                                    INNER JOIN PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS HA ON PCAH.ActionId = HA.ActionId
                                    WHERE PCAH.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
                                    AND HA.Name = 'Consultant Activated'
                                    AND PCAH.ActionDate >= @StartDate 
                                    AND PCAH.ActionDate <= @EndDate
                                    ORDER BY PCAH.ActionDate DESC, PCAH.Id DESC
                                ) SubQuery
                                WHERE SubQuery.ActionOutcome = 'Active'
                            ) OR 
                            NOT EXISTS (
                                SELECT 1
                                FROM (
                                    SELECT TOP(1)
                                        CASE
                                            WHEN HA.Name = 'Consultant Deactivated' THEN 'Inactive'
                                            ELSE 'No actions'
                                        END AS ActionOutcome
                                    FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH
                                    INNER JOIN PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS HA ON PCAH.ActionId = HA.ActionId
                                    WHERE PCAH.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
                                    AND HA.Name = 'Consultant Deactivated'
                                    AND PCAH.ActionDate < @StartDate 
                                    ORDER BY PCAH.ActionDate DESC, PCAH.Id DESC
                                ) SubQuery
                                WHERE SubQuery.ActionOutcome = 'Inactive'
                            )
                        )
                        AND PCA.ProjectConsultantAssignedId IN (SELECT ProjectConsultantAssignedId FROM ValidConsultants)
                    GROUP BY PCA.ConsultantId
                ),
                FilteredConsultants AS (
                    SELECT
                        AP.ConsultantId,
                        PCA.ProjectId,
                        AP.NumProjectsIsActive
                    FROM ActiveProjects AP
                    JOIN PROJECTS_CONSULTANTS_ASSIGNED PCA ON AP.ConsultantId = PCA.ConsultantId
                    WHERE PCA.ProjectConsultantAssignedId IN (SELECT ProjectConsultantAssignedId FROM ValidConsultants)
                )
            
                -- Count total results
                SELECT @TotalCount = COUNT(*)
                FROM (
                    SELECT
                        CONCAT(U.Name, ' ', U.LastName) AS ConsultantName,
                        CD.ConsultantId,
                        P.ProjectId,
                        P.Name AS ProjectName,
                        CASE 
                            WHEN PCA.AccessToTrackingTool = 0 AND RS.SubmissionId IS NULL THEN NULL 
                            ELSE RS.SubmissionId 
                        END AS SubmissionId,
                        CASE 
                            WHEN PCA.AccessToTrackingTool = 0 AND RS.TransactionStatusId IS NULL THEN 'Approved' 
                            ELSE TS.Name 
                        END AS TransactionStatusName,
                        CASE 
                            WHEN PCA.AccessToTrackingTool = 0 AND RS.SubmissionDate IS NULL THEN NULL 
                            ELSE RS.SubmissionDate 
                        END AS SubmissionDate,
                        CASE 
                            WHEN PCA.AccessToTrackingTool = 0 AND RS.LastSubmissionDate IS NULL THEN NULL 
                            ELSE RS.LastSubmissionDate 
                        END AS LastSubmissionDate,
                        NAS.NumApprovedSubmissions,
                        FAP.NumProjectsIsActive,
                        PCA.AccessToTrackingTool
                    FROM FilteredConsultants FAP
                    JOIN PROJECTS_CONSULTANTS_ASSIGNED PCA ON FAP.ConsultantId = PCA.ConsultantId AND FAP.ProjectId = PCA.ProjectId
                    JOIN PROJECTS P ON PCA.ProjectId = P.ProjectId
                    JOIN CONSULTANT_DETAILS CD ON PCA.ConsultantId = CD.ConsultantId
                    JOIN Users U ON CD.UserId = U.Id
                    LEFT JOIN REPORTING_MY_TIME_MOVEMENTS_SUBMISSIONS RS ON PCA.ConsultantId = RS.ConsultantId
                    AND P.ProjectId = RS.ProjectId AND RS.StartPeriodDate = @StartDate
                    AND RS.EndPeriodDate = @EndDate
                    LEFT JOIN TRANSACTION_STATUSES TS ON RS.TransactionStatusId = TS.TransactionStatusId
                    LEFT JOIN SubmissionsCounts SC ON CD.ConsultantId = SC.ConsultantId AND P.ProjectId = SC.ProjectId
                    LEFT JOIN NumApprovedSubmissionsConsistent NAS ON CD.ConsultantId = NAS.ConsultantId
                    WHERE 
                        (@ProjectId IS NULL OR P.ProjectId = @ProjectId) AND
                        (@TransactionStatusId IS NULL OR (TS.TransactionStatusId = @TransactionStatusId AND TS.TransactionStatusId IS NOT NULL)) AND
                        (@SearchText IS NULL 
                            OR LOWER(U.Name) LIKE '%' + LOWER(@SearchText) + '%' 
                            OR LOWER(U.LastName) LIKE '%' + LOWER(@SearchText) + '%' 
                            OR LOWER(CONCAT(U.Name, ' ', U.LastName)) LIKE '%' + LOWER(@SearchText) + '%') AND
                        (@PaymentPeriod IS NULL OR CD.PaymentPeriod = @PaymentPeriod)
                    GROUP BY CONCAT(U.Name, ' ', U.LastName), CD.ConsultantId, P.ProjectId, P.Name, RS.SubmissionId, RS.TransactionStatusId, TS.Name, RS.SubmissionDate, RS.LastSubmissionDate, NAS.NumApprovedSubmissions, FAP.NumProjectsIsActive, PCA.AccessToTrackingTool
                ) AS TotalRegisters;
                
                -- Request with pagination
                SELECT
                    CONCAT(U.Name, ' ', U.LastName) AS ConsultantName,
                    CD.ConsultantId,
                    P.ProjectId,
                    P.Name AS ProjectName,
                    CASE 
                        WHEN PCA.AccessToTrackingTool = 0 AND RS.SubmissionId IS NULL THEN NULL 
                        ELSE RS.SubmissionId 
                    END AS SubmissionId,
                    CASE 
                        WHEN PCA.AccessToTrackingTool = 0 AND RS.TransactionStatusId IS NULL THEN 'Approved' 
                        ELSE TS.Name 
                    END AS TransactionStatusName,
                    CASE 
                        WHEN PCA.AccessToTrackingTool = 0 AND RS.SubmissionDate IS NULL THEN NULL 
                        ELSE RS.SubmissionDate 
                    END AS SubmissionDate,
                    CASE 
                        WHEN PCA.AccessToTrackingTool = 0 AND RS.LastSubmissionDate IS NULL THEN NULL 
                        ELSE RS.LastSubmissionDate 
                    END AS LastSubmissionDate,
                    NAS.NumApprovedSubmissions,
                    FAP.NumProjectsIsActive,
                    PCA.AccessToTrackingTool
                FROM FilteredConsultants FAP
                JOIN PROJECTS_CONSULTANTS_ASSIGNED PCA ON FAP.ConsultantId = PCA.ConsultantId AND FAP.ProjectId = PCA.ProjectId
                JOIN PROJECTS P ON PCA.ProjectId = P.ProjectId
                JOIN CONSULTANT_DETAILS CD ON PCA.ConsultantId = CD.ConsultantId
                JOIN Users U ON CD.UserId = U.Id
                LEFT JOIN REPORTING_MY_TIME_MOVEMENTS_SUBMISSIONS RS ON PCA.ConsultantId = RS.ConsultantId
                AND P.ProjectId = RS.ProjectId AND RS.StartPeriodDate = @StartDate
                AND RS.EndPeriodDate = @EndDate
                LEFT JOIN TRANSACTION_STATUSES TS ON RS.TransactionStatusId = TS.TransactionStatusId
                LEFT JOIN SubmissionsCounts SC ON CD.ConsultantId = SC.ConsultantId AND P.ProjectId = SC.ProjectId
                LEFT JOIN NumApprovedSubmissionsConsistent NAS ON CD.ConsultantId = NAS.ConsultantId
                WHERE 
                    (@ProjectId IS NULL OR P.ProjectId = @ProjectId) AND
                    (@TransactionStatusId IS NULL OR (TS.TransactionStatusId = @TransactionStatusId AND TS.TransactionStatusId IS NOT NULL)) AND
                    (@SearchText IS NULL 
                        OR LOWER(U.Name) LIKE '%' + LOWER(@SearchText) + '%' 
                        OR LOWER(U.LastName) LIKE '%' + LOWER(@SearchText) + '%' 
                        OR LOWER(CONCAT(U.Name, ' ', U.LastName)) LIKE '%' + LOWER(@SearchText) + '%') AND
                    (@PaymentPeriod IS NULL OR CD.PaymentPeriod = @PaymentPeriod)
                GROUP BY CONCAT(U.Name, ' ', U.LastName), CD.ConsultantId, P.ProjectId, P.Name, RS.SubmissionId, RS.TransactionStatusId, TS.Name, RS.SubmissionDate, RS.LastSubmissionDate, NAS.NumApprovedSubmissions, FAP.NumProjectsIsActive, PCA.AccessToTrackingTool
                ORDER BY 
                    CASE WHEN @FieldToOrder = 'ConsultantName' AND @DirectionOrder = 'ASC' THEN CONCAT(U.Name, ' ', U.LastName) END ASC,
                    CASE WHEN @FieldToOrder = 'ConsultantName' AND @DirectionOrder = 'DESC' THEN CONCAT(U.Name, ' ', U.LastName) END DESC,
                    CASE WHEN @FieldToOrder = 'ProjectName' AND @DirectionOrder = 'ASC' THEN P.Name END ASC,
                    CASE WHEN @FieldToOrder = 'ProjectName' AND @DirectionOrder = 'DESC' THEN P.Name END DESC,
                    CASE WHEN @FieldToOrder = 'TransactionStatusName' AND @DirectionOrder = 'ASC' THEN TS.Name END ASC,
                    CASE WHEN @FieldToOrder = 'TransactionStatusName' AND @DirectionOrder = 'DESC' THEN TS.Name END DESC,
                    CASE WHEN @FieldToOrder = 'SubmissionDate' AND @DirectionOrder = 'ASC' THEN RS.SubmissionDate END ASC,
                    CASE WHEN @FieldToOrder = 'SubmissionDate' AND @DirectionOrder = 'DESC' THEN RS.SubmissionDate END DESC,
                    CASE WHEN @FieldToOrder = 'LastSubmissionDate' AND @DirectionOrder = 'ASC' THEN RS.LastSubmissionDate END ASC,
                    CASE WHEN @FieldToOrder = 'LastSubmissionDate' AND @DirectionOrder = 'DESC' THEN RS.LastSubmissionDate END DESC,
                    CONCAT(U.Name, ' ', U.LastName)
                OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;
            END;";

            migrationBuilder.Sql("DROP PROCEDURE IF SP_PAYMENT_SHEETS_GetAllConsultantsToPayWithFilters");
            migrationBuilder.Sql(spOriginal);
        }
    }
}
