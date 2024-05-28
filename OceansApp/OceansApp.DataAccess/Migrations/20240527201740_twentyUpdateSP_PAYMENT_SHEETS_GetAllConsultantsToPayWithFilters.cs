using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class twentyUpdateSP_PAYMENT_SHEETS_GetAllConsultantsToPayWithFilters : Migration
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
            -- Subquery to count submissions for consultants within the period
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
            
            -- Determine active consultants based on activity filters
            ActiveConsultants AS (
                SELECT
                    PCA.ConsultantId,
                    PCA.ProjectId
                FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH
                JOIN PROJECTS_CONSULTANTS_ASSIGNED PCA ON PCAH.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
                JOIN PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS HA ON PCAH.ActionId = HA.ActionId
                JOIN PROJECTS P ON PCA.ProjectId = P.ProjectId
                WHERE
                    (
                        -- Consultant Assigned First Time before or on EndDate
                        (HA.Name = 'Consultant Assigned First Time' AND PCAH.ActionDate <= @EndDate) OR
                        -- Consultant Activated before or on EndDate
                        (HA.Name = 'Consultant Activated' AND PCAH.ActionDate <= @EndDate) OR
                        -- Consultant Activated within the period
                        (HA.Name = 'Consultant Activated' AND PCAH.ActionDate >= @StartDate AND PCAH.ActionDate <= @EndDate)
                    )
                    AND NOT EXISTS (
                        -- Exclude consultants deactivated before the StartDate unless reactivated in the period
                        SELECT 1
                        FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH2
                        INNER JOIN PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS HA2 ON PCAH2.ActionId = HA2.ActionId
                        WHERE PCAH2.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
                        AND HA2.Name = 'Consultant Deactivated'
                        AND PCAH2.ActionDate < @StartDate
                        AND NOT EXISTS (
                            SELECT 1
                            FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH3
                            INNER JOIN PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS HA3 ON PCAH3.ActionId = HA3.ActionId
                            WHERE PCAH3.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
                            AND HA3.Name = 'Consultant Activated'
                            AND PCAH3.ActionDate >= @StartDate AND PCAH3.ActionDate <= @EndDate
                        )
                    )
            ),
            
            -- Filter consultants based on salary history
            FilteredConsultants AS (
                SELECT
                    AC.ConsultantId,
                    AC.ProjectId
                FROM ActiveConsultants AC
                JOIN PROJECTS_CONSULTANTS_ASSIGNED PCA ON AC.ConsultantId = PCA.ConsultantId AND AC.ProjectId = PCA.ProjectId
                WHERE
                    -- 1. Exclude consultants if they have no salary history and both HourlySalary and MonthlySalary are zero
                    NOT (
                        PCA.HourlySalary = 0 AND PCA.MonthlySalary = 0 
                        AND NOT EXISTS (
                            SELECT 1
                            FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH
                            INNER JOIN PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS HA ON PCAH.ActionId = HA.ActionId
                            WHERE PCAH.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
                            AND HA.Name IN (
                                'Hourly Salary updated',
                                'Monthly Salary updated',
                                'Consultant pricing method updated (Monthly)', 
                                'Consultant pricing method updated (Hourly)'
                            )
                        )
                    )
                    -- 2. Handle cases with 'Consultant pricing method updated (Monthly)' or 'Consultant pricing method updated (Hourly)'
                    AND (
                        NOT EXISTS (
                            SELECT 1
                            FROM (
                                SELECT 
                                    HA.Name,
                                    PCAH.ActionDate,
                                    ROW_NUMBER() OVER (ORDER BY PCAH.ActionDate DESC, PCAH.Id DESC) AS rn
                                FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH
                                INNER JOIN PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS HA ON PCAH.ActionId = HA.ActionId
                                WHERE PCAH.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
                                AND PCAH.ActionDate <= @EndDate
                                AND HA.Name IN (
                                    'Consultant pricing method updated (Monthly)',
                                    'Consultant pricing method updated (Hourly)'
                                )
                            ) SubQuery
                            WHERE SubQuery.rn = 1
                            AND (
                                (SubQuery.Name = 'Consultant pricing method updated (Monthly)' AND EXISTS (
                                    SELECT 1
                                    FROM (
                                        SELECT 
                                            PCAH.NewValue,
                                            ROW_NUMBER() OVER (ORDER BY PCAH.ActionDate DESC, PCAH.Id DESC) AS rn2
                                        FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH
                                        INNER JOIN PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS HA ON PCAH.ActionId = HA.ActionId
                                        WHERE PCAH.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
                                        AND PCAH.ActionDate <= @EndDate
                                        AND HA.Name = 'Monthly Salary updated'
                                    ) SubQuery2
                                    WHERE SubQuery2.rn2 = 1 AND SubQuery2.NewValue = 0
                                ))
                                OR (SubQuery.Name = 'Consultant pricing method updated (Hourly)' AND EXISTS (
                                    SELECT 1
                                    FROM (
                                        SELECT 
                                            PCAH.NewValue,
                                            ROW_NUMBER() OVER (ORDER BY PCAH.ActionDate DESC, PCAH.Id DESC) AS rn2
                                        FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH
                                        INNER JOIN PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS HA ON PCAH.ActionId = HA.ActionId
                                        WHERE PCAH.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
                                        AND PCAH.ActionDate <= @EndDate
                                        AND HA.Name = 'Hourly Salary updated'
                                    ) SubQuery2
                                    WHERE SubQuery2.rn2 = 1 AND SubQuery2.NewValue = 0
                                ))
                            )
                        )
                    )
                    -- 3. Handle cases with only 'Hourly Salary updated' and 'Monthly Salary updated' without 'Consultant pricing method updated'
                    AND (
                        NOT EXISTS (
                            SELECT 1
                            FROM (
                                SELECT 
                                    HA.Name,
                                    PCAH.NewValue,
                                    ROW_NUMBER() OVER (ORDER BY PCAH.ActionDate DESC, PCAH.Id DESC) AS rn
                                FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH
                                INNER JOIN PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS HA ON PCAH.ActionId = HA.ActionId
                                WHERE PCAH.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
                                AND PCAH.ActionDate <= @EndDate
                                AND HA.Name IN ('Hourly Salary updated', 'Monthly Salary updated')
                                AND NOT EXISTS (
                                    SELECT 1
                                    FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH2
                                    INNER JOIN PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS HA2 ON PCAH2.ActionId = HA2.ActionId
                                    WHERE PCAH2.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
                                    AND HA2.Name IN ('Consultant pricing method updated (Monthly)', 'Consultant pricing method updated (Hourly)')
                                )
                            ) SubQuery
                            WHERE SubQuery.rn = 1
                            AND SubQuery.NewValue = 0
                        )
                    )
            )
            
            ,
            
            -- Calculate the number of approved submissions for each consultant
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
                    WHERE EXISTS (
                        SELECT 1
                        FROM FilteredConsultants FC
                        WHERE FC.ConsultantId = PCA.ConsultantId
                        AND FC.ProjectId = PCA.ProjectId
                    )
                ) AS SubQuery
                GROUP BY ConsultantId
            ),
            
            -- Calculate the number of active projects for each consultant
            ActiveProjectsCount AS (
                SELECT
                    FC.ConsultantId,
                    COUNT(DISTINCT FC.ProjectId) AS NumProjectsIsActive
                FROM FilteredConsultants FC
                GROUP BY FC.ConsultantId
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
                APC.NumProjectsIsActive,
                PCA.AccessToTrackingTool
            FROM FilteredConsultants FC
            JOIN PROJECTS_CONSULTANTS_ASSIGNED PCA ON FC.ConsultantId = PCA.ConsultantId AND FC.ProjectId = PCA.ProjectId
            JOIN PROJECTS P ON PCA.ProjectId = P.ProjectId
            JOIN CONSULTANT_DETAILS CD ON PCA.ConsultantId = CD.ConsultantId
            JOIN Users U ON CD.UserId = U.Id
            LEFT JOIN REPORTING_MY_TIME_MOVEMENTS_SUBMISSIONS RS ON PCA.ConsultantId = RS.ConsultantId
            AND P.ProjectId = RS.ProjectId AND RS.StartPeriodDate = @StartDate
            AND RS.EndPeriodDate = @EndDate
            LEFT JOIN TRANSACTION_STATUSES TS ON RS.TransactionStatusId = TS.TransactionStatusId
            LEFT JOIN SubmissionsCounts SC ON CD.ConsultantId = SC.ConsultantId AND P.ProjectId = SC.ProjectId
            LEFT JOIN NumApprovedSubmissionsConsistent NAS ON CD.ConsultantId = NAS.ConsultantId
            LEFT JOIN ActiveProjectsCount APC ON FC.ConsultantId = APC.ConsultantId
            WHERE 
                (@ProjectId IS NULL OR P.ProjectId = @ProjectId) AND
                (@TransactionStatusId IS NULL OR (TS.TransactionStatusId = @TransactionStatusId AND TS.TransactionStatusId IS NOT NULL)) AND
                (@SearchText IS NULL 
                    OR LOWER(U.Name) LIKE '%' + LOWER(@SearchText) + '%' 
                    OR LOWER(U.LastName) LIKE '%' + LOWER(@SearchText) + '%' 
                    OR LOWER(CONCAT(U.Name, ' ', U.LastName)) LIKE '%' + LOWER(@SearchText) + '%') AND
                (@PaymentPeriod IS NULL OR CD.PaymentPeriod = @PaymentPeriod)
            GROUP BY CONCAT(U.Name, ' ', U.LastName), CD.ConsultantId, P.ProjectId, P.Name, RS.SubmissionId, RS.TransactionStatusId, TS.Name, RS.SubmissionDate, RS.LastSubmissionDate, NAS.NumApprovedSubmissions, APC.NumProjectsIsActive, PCA.AccessToTrackingTool
            ) AS TotalRegisters;
            
                        -- Request with pagination
                                   
            -- Subquery to count submissions for consultants within the period
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
            
            -- Determine active consultants based on activity filters
            ActiveConsultants AS (
                SELECT
                    PCA.ConsultantId,
                    PCA.ProjectId
                FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH
                JOIN PROJECTS_CONSULTANTS_ASSIGNED PCA ON PCAH.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
                JOIN PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS HA ON PCAH.ActionId = HA.ActionId
                JOIN PROJECTS P ON PCA.ProjectId = P.ProjectId
                WHERE
                    (
                        -- Consultant Assigned First Time before or on EndDate
                        (HA.Name = 'Consultant Assigned First Time' AND PCAH.ActionDate <= @EndDate) OR
                        -- Consultant Activated before or on EndDate
                        (HA.Name = 'Consultant Activated' AND PCAH.ActionDate <= @EndDate) OR
                        -- Consultant Activated within the period
                        (HA.Name = 'Consultant Activated' AND PCAH.ActionDate >= @StartDate AND PCAH.ActionDate <= @EndDate)
                    )
                    AND NOT EXISTS (
                        -- Exclude consultants deactivated before the StartDate unless reactivated in the period
                        SELECT 1
                        FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH2
                        INNER JOIN PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS HA2 ON PCAH2.ActionId = HA2.ActionId
                        WHERE PCAH2.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
                        AND HA2.Name = 'Consultant Deactivated'
                        AND PCAH2.ActionDate < @StartDate
                        AND NOT EXISTS (
                            SELECT 1
                            FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH3
                            INNER JOIN PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS HA3 ON PCAH3.ActionId = HA3.ActionId
                            WHERE PCAH3.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
                            AND HA3.Name = 'Consultant Activated'
                            AND PCAH3.ActionDate >= @StartDate AND PCAH3.ActionDate <= @EndDate
                        )
                    )
            ),
            
            -- Filter consultants based on salary history
            FilteredConsultants AS (
                SELECT
                    AC.ConsultantId,
                    AC.ProjectId
                FROM ActiveConsultants AC
                JOIN PROJECTS_CONSULTANTS_ASSIGNED PCA ON AC.ConsultantId = PCA.ConsultantId AND AC.ProjectId = PCA.ProjectId
                WHERE
                    -- 1. Exclude consultants if they have no salary history and both HourlySalary and MonthlySalary are zero
                    NOT (
                        PCA.HourlySalary = 0 AND PCA.MonthlySalary = 0 
                        AND NOT EXISTS (
                            SELECT 1
                            FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH
                            INNER JOIN PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS HA ON PCAH.ActionId = HA.ActionId
                            WHERE PCAH.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
                            AND HA.Name IN (
                                'Hourly Salary updated',
                                'Monthly Salary updated',
                                'Consultant pricing method updated (Monthly)', 
                                'Consultant pricing method updated (Hourly)'
                            )
                            AND PCAH.ActionDate <= @EndDate
                        )
                    )
                    -- 2. Handle cases with 'Consultant pricing method updated (Monthly)' or 'Consultant pricing method updated (Hourly)'
                    AND (
                        NOT EXISTS (
                            SELECT 1
                            FROM (
                                SELECT 
                                    HA.Name,
                                    PCAH.ActionDate,
                                    ROW_NUMBER() OVER (ORDER BY PCAH.ActionDate DESC, PCAH.Id DESC) AS rn
                                FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH
                                INNER JOIN PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS HA ON PCAH.ActionId = HA.ActionId
                                WHERE PCAH.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
                                AND PCAH.ActionDate <= @EndDate
                                AND HA.Name IN (
                                    'Consultant pricing method updated (Monthly)',
                                    'Consultant pricing method updated (Hourly)'
                                )
                            ) SubQuery
                            WHERE SubQuery.rn = 1
                            AND (
                                (SubQuery.Name = 'Consultant pricing method updated (Monthly)' AND EXISTS (
                                    SELECT 1
                                    FROM (
                                        SELECT 
                                            PCAH.NewValue,
                                            ROW_NUMBER() OVER (ORDER BY PCAH.ActionDate DESC, PCAH.Id DESC) AS rn2
                                        FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH
                                        INNER JOIN PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS HA ON PCAH.ActionId = HA.ActionId
                                        WHERE PCAH.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
                                        AND PCAH.ActionDate <= @EndDate
                                        AND HA.Name = 'Monthly Salary updated'
                                    ) SubQuery2
                                    WHERE SubQuery2.rn2 = 1 AND SubQuery2.NewValue = 0
                                ))
                                OR (SubQuery.Name = 'Consultant pricing method updated (Hourly)' AND EXISTS (
                                    SELECT 1
                                    FROM (
                                        SELECT 
                                            PCAH.NewValue,
                                            ROW_NUMBER() OVER (ORDER BY PCAH.ActionDate DESC, PCAH.Id DESC) AS rn2
                                        FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH
                                        INNER JOIN PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS HA ON PCAH.ActionId = HA.ActionId
                                        WHERE PCAH.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
                                        AND PCAH.ActionDate <= @EndDate
                                        AND HA.Name = 'Hourly Salary updated'
                                    ) SubQuery2
                                    WHERE SubQuery2.rn2 = 1 AND SubQuery2.NewValue = 0
                                ))
                            )
                        )
                    )
                    -- 3. Handle cases with only 'Hourly Salary updated' and 'Monthly Salary updated' without 'Consultant pricing method updated'
                    AND (
                    NOT EXISTS (
                        SELECT 1
                        FROM (
                            SELECT 
                                HA.Name,
                                PCAH.NewValue,
                                ROW_NUMBER() OVER (ORDER BY PCAH.ActionDate DESC, PCAH.Id DESC) AS rn
                            FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH
                            INNER JOIN PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS HA ON PCAH.ActionId = HA.ActionId
                            WHERE PCAH.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
                            AND PCAH.ActionDate <= @EndDate
                            AND HA.Name IN ('Hourly Salary updated', 'Monthly Salary updated')
                            AND NOT EXISTS (
                                SELECT 1
                                FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH2
                                INNER JOIN PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS HA2 ON PCAH2.ActionId = HA2.ActionId
                                WHERE PCAH2.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
                                AND HA2.Name IN ('Consultant pricing method updated (Monthly)', 'Consultant pricing method updated (Hourly)')
                                AND PCAH.ActionDate <= @EndDate
                            )
                        ) SubQuery
                        WHERE SubQuery.rn = 1
                        AND SubQuery.NewValue = 0
                    )
                )
            )
            ,
            
            -- Calculate the number of approved submissions for each consultant
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
                    WHERE EXISTS (
                        SELECT 1
                        FROM FilteredConsultants FC
                        WHERE FC.ConsultantId = PCA.ConsultantId
                        AND FC.ProjectId = PCA.ProjectId
                    )
                ) AS SubQuery
                GROUP BY ConsultantId
            ),
            
            -- Calculate the number of active projects for each consultant
            ActiveProjectsCount AS (
                SELECT
                    FC.ConsultantId,
                    COUNT(DISTINCT FC.ProjectId) AS NumProjectsIsActive
                FROM FilteredConsultants FC
                GROUP BY FC.ConsultantId
            )

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
                APC.NumProjectsIsActive,
                PCA.AccessToTrackingTool
            FROM FilteredConsultants FC
            JOIN PROJECTS_CONSULTANTS_ASSIGNED PCA ON FC.ConsultantId = PCA.ConsultantId AND FC.ProjectId = PCA.ProjectId
            JOIN PROJECTS P ON PCA.ProjectId = P.ProjectId
            JOIN CONSULTANT_DETAILS CD ON PCA.ConsultantId = CD.ConsultantId
            JOIN Users U ON CD.UserId = U.Id
            LEFT JOIN REPORTING_MY_TIME_MOVEMENTS_SUBMISSIONS RS ON PCA.ConsultantId = RS.ConsultantId
            AND P.ProjectId = RS.ProjectId AND RS.StartPeriodDate = @StartDate
            AND RS.EndPeriodDate = @EndDate
            LEFT JOIN TRANSACTION_STATUSES TS ON RS.TransactionStatusId = TS.TransactionStatusId
            LEFT JOIN SubmissionsCounts SC ON CD.ConsultantId = SC.ConsultantId AND P.ProjectId = SC.ProjectId
            LEFT JOIN NumApprovedSubmissionsConsistent NAS ON CD.ConsultantId = NAS.ConsultantId
            LEFT JOIN ActiveProjectsCount APC ON FC.ConsultantId = APC.ConsultantId
            WHERE 
                (@ProjectId IS NULL OR P.ProjectId = @ProjectId) AND
                (@TransactionStatusId IS NULL OR (TS.TransactionStatusId = @TransactionStatusId AND TS.TransactionStatusId IS NOT NULL)) AND
                (@SearchText IS NULL 
                    OR LOWER(U.Name) LIKE '%' + LOWER(@SearchText) + '%' 
                    OR LOWER(U.LastName) LIKE '%' + LOWER(@SearchText) + '%' 
                    OR LOWER(CONCAT(U.Name, ' ', U.LastName)) LIKE '%' + LOWER(@SearchText) + '%') AND
                (@PaymentPeriod IS NULL OR CD.PaymentPeriod = @PaymentPeriod)
            GROUP BY CONCAT(U.Name, ' ', U.LastName), CD.ConsultantId, P.ProjectId, P.Name, RS.SubmissionId, RS.TransactionStatusId, TS.Name, RS.SubmissionDate, RS.LastSubmissionDate, NAS.NumApprovedSubmissions, APC.NumProjectsIsActive, PCA.AccessToTrackingTool
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
            -- Subquery to count submissions for consultants within the period
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
            
            -- Determine active consultants based on activity filters
            ActiveConsultants AS (
                SELECT
                    PCA.ConsultantId,
                    PCA.ProjectId
                FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH
                JOIN PROJECTS_CONSULTANTS_ASSIGNED PCA ON PCAH.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
                JOIN PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS HA ON PCAH.ActionId = HA.ActionId
                JOIN PROJECTS P ON PCA.ProjectId = P.ProjectId
                WHERE
                    (
                        -- Consultant Assigned First Time before or on EndDate
                        (HA.Name = 'Consultant Assigned First Time' AND PCAH.ActionDate <= @EndDate) OR
                        -- Consultant Activated before or on EndDate
                        (HA.Name = 'Consultant Activated' AND PCAH.ActionDate <= @EndDate) OR
                        -- Consultant Activated within the period
                        (HA.Name = 'Consultant Activated' AND PCAH.ActionDate >= @StartDate AND PCAH.ActionDate <= @EndDate)
                    )
                    AND NOT EXISTS (
                        -- Exclude consultants deactivated before the StartDate unless reactivated in the period
                        SELECT 1
                        FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH2
                        INNER JOIN PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS HA2 ON PCAH2.ActionId = HA2.ActionId
                        WHERE PCAH2.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
                        AND HA2.Name = 'Consultant Deactivated'
                        AND PCAH2.ActionDate < @StartDate
                        AND NOT EXISTS (
                            SELECT 1
                            FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH3
                            INNER JOIN PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS HA3 ON PCAH3.ActionId = HA3.ActionId
                            WHERE PCAH3.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
                            AND HA3.Name = 'Consultant Activated'
                            AND PCAH3.ActionDate >= @StartDate AND PCAH3.ActionDate <= @EndDate
                        )
                    )
            ),
            
            -- Filter consultants based on salary history
            FilteredConsultants AS (
                SELECT
                    AC.ConsultantId,
                    AC.ProjectId
                FROM ActiveConsultants AC
                JOIN PROJECTS_CONSULTANTS_ASSIGNED PCA ON AC.ConsultantId = PCA.ConsultantId AND AC.ProjectId = PCA.ProjectId
                WHERE
                    -- 1. Exclude consultants if they have no salary history and both HourlySalary and MonthlySalary are zero
                    NOT (
                        PCA.HourlySalary = 0 AND PCA.MonthlySalary = 0 
                        AND NOT EXISTS (
                            SELECT 1
                            FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH
                            INNER JOIN PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS HA ON PCAH.ActionId = HA.ActionId
                            WHERE PCAH.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
                            AND HA.Name IN (
                                'Hourly Salary updated',
                                'Monthly Salary updated',
                                'Consultant pricing method updated (Monthly)', 
                                'Consultant pricing method updated (Hourly)'
                            )
                        )
                    )
                    -- 2. Handle cases with 'Consultant pricing method updated (Monthly)' or 'Consultant pricing method updated (Hourly)'
                    AND (
                        NOT EXISTS (
                            SELECT 1
                            FROM (
                                SELECT 
                                    HA.Name,
                                    PCAH.ActionDate,
                                    ROW_NUMBER() OVER (ORDER BY PCAH.ActionDate DESC, PCAH.Id DESC) AS rn
                                FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH
                                INNER JOIN PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS HA ON PCAH.ActionId = HA.ActionId
                                WHERE PCAH.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
                                AND PCAH.ActionDate <= @EndDate
                                AND HA.Name IN (
                                    'Consultant pricing method updated (Monthly)',
                                    'Consultant pricing method updated (Hourly)'
                                )
                            ) SubQuery
                            WHERE SubQuery.rn = 1
                            AND (
                                (SubQuery.Name = 'Consultant pricing method updated (Monthly)' AND EXISTS (
                                    SELECT 1
                                    FROM (
                                        SELECT 
                                            PCAH.NewValue,
                                            ROW_NUMBER() OVER (ORDER BY PCAH.ActionDate DESC, PCAH.Id DESC) AS rn2
                                        FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH
                                        INNER JOIN PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS HA ON PCAH.ActionId = HA.ActionId
                                        WHERE PCAH.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
                                        AND PCAH.ActionDate <= @EndDate
                                        AND HA.Name = 'Monthly Salary updated'
                                    ) SubQuery2
                                    WHERE SubQuery2.rn2 = 1 AND SubQuery2.NewValue = 0
                                ))
                                OR (SubQuery.Name = 'Consultant pricing method updated (Hourly)' AND EXISTS (
                                    SELECT 1
                                    FROM (
                                        SELECT 
                                            PCAH.NewValue,
                                            ROW_NUMBER() OVER (ORDER BY PCAH.ActionDate DESC, PCAH.Id DESC) AS rn2
                                        FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH
                                        INNER JOIN PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS HA ON PCAH.ActionId = HA.ActionId
                                        WHERE PCAH.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
                                        AND PCAH.ActionDate <= @EndDate
                                        AND HA.Name = 'Hourly Salary updated'
                                    ) SubQuery2
                                    WHERE SubQuery2.rn2 = 1 AND SubQuery2.NewValue = 0
                                ))
                            )
                        )
                    )
                    -- 3. Handle cases with only 'Hourly Salary updated' and 'Monthly Salary updated' without 'Consultant pricing method updated'
                    AND (
                        NOT EXISTS (
                            SELECT 1
                            FROM (
                                SELECT 
                                    HA.Name,
                                    PCAH.NewValue,
                                    ROW_NUMBER() OVER (ORDER BY PCAH.ActionDate DESC, PCAH.Id DESC) AS rn
                                FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH
                                INNER JOIN PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS HA ON PCAH.ActionId = HA.ActionId
                                WHERE PCAH.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
                                AND PCAH.ActionDate <= @EndDate
                                AND HA.Name IN ('Hourly Salary updated', 'Monthly Salary updated')
                                AND NOT EXISTS (
                                    SELECT 1
                                    FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH2
                                    INNER JOIN PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS HA2 ON PCAH2.ActionId = HA2.ActionId
                                    WHERE PCAH2.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
                                    AND HA2.Name IN ('Consultant pricing method updated (Monthly)', 'Consultant pricing method updated (Hourly)')
                                )
                            ) SubQuery
                            WHERE SubQuery.rn = 1
                            AND SubQuery.NewValue = 0
                        )
                    )
            )
            
            ,
            
            -- Calculate the number of approved submissions for each consultant
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
                    WHERE EXISTS (
                        SELECT 1
                        FROM FilteredConsultants FC
                        WHERE FC.ConsultantId = PCA.ConsultantId
                        AND FC.ProjectId = PCA.ProjectId
                    )
                ) AS SubQuery
                GROUP BY ConsultantId
            ),
            
            -- Calculate the number of active projects for each consultant
            ActiveProjectsCount AS (
                SELECT
                    FC.ConsultantId,
                    COUNT(DISTINCT FC.ProjectId) AS NumProjectsIsActive
                FROM FilteredConsultants FC
                GROUP BY FC.ConsultantId
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
                APC.NumProjectsIsActive,
                PCA.AccessToTrackingTool
            FROM FilteredConsultants FC
            JOIN PROJECTS_CONSULTANTS_ASSIGNED PCA ON FC.ConsultantId = PCA.ConsultantId AND FC.ProjectId = PCA.ProjectId
            JOIN PROJECTS P ON PCA.ProjectId = P.ProjectId
            JOIN CONSULTANT_DETAILS CD ON PCA.ConsultantId = CD.ConsultantId
            JOIN Users U ON CD.UserId = U.Id
            LEFT JOIN REPORTING_MY_TIME_MOVEMENTS_SUBMISSIONS RS ON PCA.ConsultantId = RS.ConsultantId
            AND P.ProjectId = RS.ProjectId AND RS.StartPeriodDate = @StartDate
            AND RS.EndPeriodDate = @EndDate
            LEFT JOIN TRANSACTION_STATUSES TS ON RS.TransactionStatusId = TS.TransactionStatusId
            LEFT JOIN SubmissionsCounts SC ON CD.ConsultantId = SC.ConsultantId AND P.ProjectId = SC.ProjectId
            LEFT JOIN NumApprovedSubmissionsConsistent NAS ON CD.ConsultantId = NAS.ConsultantId
            LEFT JOIN ActiveProjectsCount APC ON FC.ConsultantId = APC.ConsultantId
            WHERE 
                (@ProjectId IS NULL OR P.ProjectId = @ProjectId) AND
                (@TransactionStatusId IS NULL OR (TS.TransactionStatusId = @TransactionStatusId AND TS.TransactionStatusId IS NOT NULL)) AND
                (@SearchText IS NULL 
                    OR LOWER(U.Name) LIKE '%' + LOWER(@SearchText) + '%' 
                    OR LOWER(U.LastName) LIKE '%' + LOWER(@SearchText) + '%' 
                    OR LOWER(CONCAT(U.Name, ' ', U.LastName)) LIKE '%' + LOWER(@SearchText) + '%') AND
                (@PaymentPeriod IS NULL OR CD.PaymentPeriod = @PaymentPeriod)
            GROUP BY CONCAT(U.Name, ' ', U.LastName), CD.ConsultantId, P.ProjectId, P.Name, RS.SubmissionId, RS.TransactionStatusId, TS.Name, RS.SubmissionDate, RS.LastSubmissionDate, NAS.NumApprovedSubmissions, APC.NumProjectsIsActive, PCA.AccessToTrackingTool
            ) AS TotalRegisters;
            
                        -- Request with pagination
                                   
            -- Subquery to count submissions for consultants within the period
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
            
            -- Determine active consultants based on activity filters
            ActiveConsultants AS (
                SELECT
                    PCA.ConsultantId,
                    PCA.ProjectId
                FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH
                JOIN PROJECTS_CONSULTANTS_ASSIGNED PCA ON PCAH.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
                JOIN PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS HA ON PCAH.ActionId = HA.ActionId
                JOIN PROJECTS P ON PCA.ProjectId = P.ProjectId
                WHERE
                    (
                        -- Consultant Assigned First Time before or on EndDate
                        (HA.Name = 'Consultant Assigned First Time' AND PCAH.ActionDate <= @EndDate) OR
                        -- Consultant Activated before or on EndDate
                        (HA.Name = 'Consultant Activated' AND PCAH.ActionDate <= @EndDate) OR
                        -- Consultant Activated within the period
                        (HA.Name = 'Consultant Activated' AND PCAH.ActionDate >= @StartDate AND PCAH.ActionDate <= @EndDate)
                    )
                    AND NOT EXISTS (
                        -- Exclude consultants deactivated before the StartDate unless reactivated in the period
                        SELECT 1
                        FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH2
                        INNER JOIN PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS HA2 ON PCAH2.ActionId = HA2.ActionId
                        WHERE PCAH2.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
                        AND HA2.Name = 'Consultant Deactivated'
                        AND PCAH2.ActionDate < @StartDate
                        AND NOT EXISTS (
                            SELECT 1
                            FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH3
                            INNER JOIN PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS HA3 ON PCAH3.ActionId = HA3.ActionId
                            WHERE PCAH3.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
                            AND HA3.Name = 'Consultant Activated'
                            AND PCAH3.ActionDate >= @StartDate AND PCAH3.ActionDate <= @EndDate
                        )
                    )
            ),
            
            -- Filter consultants based on salary history
            FilteredConsultants AS (
                SELECT
                    AC.ConsultantId,
                    AC.ProjectId
                FROM ActiveConsultants AC
                JOIN PROJECTS_CONSULTANTS_ASSIGNED PCA ON AC.ConsultantId = PCA.ConsultantId AND AC.ProjectId = PCA.ProjectId
                WHERE
                    -- 1. Exclude consultants if they have no salary history and both HourlySalary and MonthlySalary are zero
                    NOT (
                        PCA.HourlySalary = 0 AND PCA.MonthlySalary = 0 
                        AND NOT EXISTS (
                            SELECT 1
                            FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH
                            INNER JOIN PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS HA ON PCAH.ActionId = HA.ActionId
                            WHERE PCAH.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
                            AND HA.Name IN (
                                'Hourly Salary updated',
                                'Monthly Salary updated',
                                'Consultant pricing method updated (Monthly)', 
                                'Consultant pricing method updated (Hourly)'
                            )
                        )
                    )
                    -- 2. Handle cases with 'Consultant pricing method updated (Monthly)' or 'Consultant pricing method updated (Hourly)'
                    AND (
                        NOT EXISTS (
                            SELECT 1
                            FROM (
                                SELECT 
                                    HA.Name,
                                    PCAH.ActionDate,
                                    ROW_NUMBER() OVER (ORDER BY PCAH.ActionDate DESC, PCAH.Id DESC) AS rn
                                FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH
                                INNER JOIN PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS HA ON PCAH.ActionId = HA.ActionId
                                WHERE PCAH.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
                                AND PCAH.ActionDate <= @EndDate
                                AND HA.Name IN (
                                    'Consultant pricing method updated (Monthly)',
                                    'Consultant pricing method updated (Hourly)'
                                )
                            ) SubQuery
                            WHERE SubQuery.rn = 1
                            AND (
                                (SubQuery.Name = 'Consultant pricing method updated (Monthly)' AND EXISTS (
                                    SELECT 1
                                    FROM (
                                        SELECT 
                                            PCAH.NewValue,
                                            ROW_NUMBER() OVER (ORDER BY PCAH.ActionDate DESC, PCAH.Id DESC) AS rn2
                                        FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH
                                        INNER JOIN PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS HA ON PCAH.ActionId = HA.ActionId
                                        WHERE PCAH.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
                                        AND PCAH.ActionDate <= @EndDate
                                        AND HA.Name = 'Monthly Salary updated'
                                    ) SubQuery2
                                    WHERE SubQuery2.rn2 = 1 AND SubQuery2.NewValue = 0
                                ))
                                OR (SubQuery.Name = 'Consultant pricing method updated (Hourly)' AND EXISTS (
                                    SELECT 1
                                    FROM (
                                        SELECT 
                                            PCAH.NewValue,
                                            ROW_NUMBER() OVER (ORDER BY PCAH.ActionDate DESC, PCAH.Id DESC) AS rn2
                                        FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH
                                        INNER JOIN PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS HA ON PCAH.ActionId = HA.ActionId
                                        WHERE PCAH.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
                                        AND PCAH.ActionDate <= @EndDate
                                        AND HA.Name = 'Hourly Salary updated'
                                    ) SubQuery2
                                    WHERE SubQuery2.rn2 = 1 AND SubQuery2.NewValue = 0
                                ))
                            )
                        )
                    )
                    -- 3. Handle cases with only 'Hourly Salary updated' and 'Monthly Salary updated' without 'Consultant pricing method updated'
                    AND (
                        NOT EXISTS (
                            SELECT 1
                            FROM (
                                SELECT 
                                    HA.Name,
                                    PCAH.NewValue,
                                    ROW_NUMBER() OVER (ORDER BY PCAH.ActionDate DESC, PCAH.Id DESC) AS rn
                                FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH
                                INNER JOIN PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS HA ON PCAH.ActionId = HA.ActionId
                                WHERE PCAH.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
                                AND PCAH.ActionDate <= @EndDate
                                AND HA.Name IN ('Hourly Salary updated', 'Monthly Salary updated')
                                AND NOT EXISTS (
                                    SELECT 1
                                    FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH2
                                    INNER JOIN PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS HA2 ON PCAH2.ActionId = HA2.ActionId
                                    WHERE PCAH2.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
                                    AND HA2.Name IN ('Consultant pricing method updated (Monthly)', 'Consultant pricing method updated (Hourly)')
                                )
                            ) SubQuery
                            WHERE SubQuery.rn = 1
                            AND SubQuery.NewValue = 0
                        )
                    )
            )
            
            ,
            
            -- Calculate the number of approved submissions for each consultant
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
                    WHERE EXISTS (
                        SELECT 1
                        FROM FilteredConsultants FC
                        WHERE FC.ConsultantId = PCA.ConsultantId
                        AND FC.ProjectId = PCA.ProjectId
                    )
                ) AS SubQuery
                GROUP BY ConsultantId
            ),
            
            -- Calculate the number of active projects for each consultant
            ActiveProjectsCount AS (
                SELECT
                    FC.ConsultantId,
                    COUNT(DISTINCT FC.ProjectId) AS NumProjectsIsActive
                FROM FilteredConsultants FC
                GROUP BY FC.ConsultantId
            )

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
                APC.NumProjectsIsActive,
                PCA.AccessToTrackingTool
            FROM FilteredConsultants FC
            JOIN PROJECTS_CONSULTANTS_ASSIGNED PCA ON FC.ConsultantId = PCA.ConsultantId AND FC.ProjectId = PCA.ProjectId
            JOIN PROJECTS P ON PCA.ProjectId = P.ProjectId
            JOIN CONSULTANT_DETAILS CD ON PCA.ConsultantId = CD.ConsultantId
            JOIN Users U ON CD.UserId = U.Id
            LEFT JOIN REPORTING_MY_TIME_MOVEMENTS_SUBMISSIONS RS ON PCA.ConsultantId = RS.ConsultantId
            AND P.ProjectId = RS.ProjectId AND RS.StartPeriodDate = @StartDate
            AND RS.EndPeriodDate = @EndDate
            LEFT JOIN TRANSACTION_STATUSES TS ON RS.TransactionStatusId = TS.TransactionStatusId
            LEFT JOIN SubmissionsCounts SC ON CD.ConsultantId = SC.ConsultantId AND P.ProjectId = SC.ProjectId
            LEFT JOIN NumApprovedSubmissionsConsistent NAS ON CD.ConsultantId = NAS.ConsultantId
            LEFT JOIN ActiveProjectsCount APC ON FC.ConsultantId = APC.ConsultantId
            WHERE 
                (@ProjectId IS NULL OR P.ProjectId = @ProjectId) AND
                (@TransactionStatusId IS NULL OR (TS.TransactionStatusId = @TransactionStatusId AND TS.TransactionStatusId IS NOT NULL)) AND
                (@SearchText IS NULL 
                    OR LOWER(U.Name) LIKE '%' + LOWER(@SearchText) + '%' 
                    OR LOWER(U.LastName) LIKE '%' + LOWER(@SearchText) + '%' 
                    OR LOWER(CONCAT(U.Name, ' ', U.LastName)) LIKE '%' + LOWER(@SearchText) + '%') AND
                (@PaymentPeriod IS NULL OR CD.PaymentPeriod = @PaymentPeriod)
            GROUP BY CONCAT(U.Name, ' ', U.LastName), CD.ConsultantId, P.ProjectId, P.Name, RS.SubmissionId, RS.TransactionStatusId, TS.Name, RS.SubmissionDate, RS.LastSubmissionDate, NAS.NumApprovedSubmissions, APC.NumProjectsIsActive, PCA.AccessToTrackingTool
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

            migrationBuilder.Sql("DROP PROCEDURE IF SP_PAYMENT_SHEETS_GetAllConsultantsToPayWithFilters");
            migrationBuilder.Sql(spOriginal);
        }
    }
}
