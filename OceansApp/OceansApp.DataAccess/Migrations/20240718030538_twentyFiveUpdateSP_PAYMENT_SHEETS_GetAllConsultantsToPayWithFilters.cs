using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class twentyFiveUpdateSP_PAYMENT_SHEETS_GetAllConsultantsToPayWithFilters : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //UPDATE STORED PROCEDURE
            var sp = @"CREATE PROCEDURE SP_PAYMENT_SHEETS_GetAllConsultantsToPayWithFilters
            @SearchText NVARCHAR(255),
            @StartDate DATE,
            @EndDate DATE,
            @TransactionStatusName NVARCHAR(80),
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
                AND (SCRMTS.StartPeriodDate >= @StartDate
                OR SCRMTS.EndPeriodDate <= @EndDate)
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
                        -- 1. Consultant Assigned First Time before or on EndDate
                        (HA.Name = 'Consultant Assigned First Time' AND PCAH.ActionDate <= @EndDate) OR
                        -- 2. Consultant Activated before or on EndDate
                        (HA.Name = 'Consultant Activated' AND PCAH.ActionDate <= @EndDate) OR
                        -- 3. Consultant Activated within the period
                        (HA.Name = 'Consultant Activated' AND PCAH.ActionDate >= @StartDate AND PCAH.ActionDate <= @EndDate)
                    )
                    AND NOT EXISTS (
                        -- Exclude consultants whose most recent history before StartDate is 'Consultant Deactivated' 
                        -- and no 'Consultant Activated' action within the period
                        SELECT 1
                        FROM (
                            SELECT TOP 1 HA2.Name, PCAH2.ActionDate
                            FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH2
                            INNER JOIN PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS HA2 ON PCAH2.ActionId = HA2.ActionId
                            WHERE PCAH2.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
                            AND PCAH2.ActionDate < @StartDate
                            ORDER BY PCAH2.ActionDate DESC, PCAH2.Id DESC
                        ) SubQuery
                        WHERE SubQuery.Name = 'Consultant Deactivated'
                        AND NOT EXISTS (
                            SELECT 1
                            FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH3
                            INNER JOIN PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS HA3 ON PCAH3.ActionId = HA3.ActionId
                            WHERE PCAH3.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
                            AND HA3.Name = 'Consultant Activated'
                            AND PCAH3.ActionDate >= @StartDate AND PCAH3.ActionDate <= @EndDate
                        )
                    )
                    AND NOT EXISTS (
                        -- Exclude consultants who have a disabled tracking record within the period
                        SELECT 1
                        FROM PROJECTS_CONSULTANTS_PERIODS_DISABLED_TRACKINGS PCPDT
                        WHERE PCPDT.ProjectId = PCA.ProjectId
                        AND PCPDT.ConsultantId = PCA.ConsultantId
                        AND PCPDT.StartPeriodDate >= @StartDate
                        AND PCPDT.EndPeriodDate <= @EndDate
                        AND PCPDT.EndPeriodDate = @EndDate
                    )
            ),
            -- Filtered active consultants
            FilteredConsultants AS (
                SELECT
                    AC.ConsultantId,
                    AC.ProjectId
                FROM ActiveConsultants AC
                JOIN PROJECTS_CONSULTANTS_ASSIGNED PCA ON AC.ConsultantId = PCA.ConsultantId AND AC.ProjectId = PCA.ProjectId
            ),
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
            ),
            -- Main query with pagination and total count
            PagedResults AS (
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
                        ELSE ISNULL(TS.Name, 'Pending') 
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
                    PCA.AccessToTrackingTool,
                    ROW_NUMBER() OVER (PARTITION BY CONCAT(U.Name, ' ', U.LastName), P.Name ORDER BY P.Name) AS RowNumber
                FROM FilteredConsultants FC
                JOIN PROJECTS_CONSULTANTS_ASSIGNED PCA ON FC.ConsultantId = PCA.ConsultantId AND FC.ProjectId = PCA.ProjectId
                JOIN PROJECTS P ON PCA.ProjectId = P.ProjectId
                JOIN CONSULTANT_DETAILS CD ON PCA.ConsultantId = CD.ConsultantId
                JOIN Users U ON CD.UserId = U.Id
                LEFT JOIN REPORTING_MY_TIME_MOVEMENTS_SUBMISSIONS RS ON PCA.ConsultantId = RS.ConsultantId
                AND P.ProjectId = RS.ProjectId AND (RS.StartPeriodDate >= @StartDate OR RS.EndPeriodDate <= @EndDate)
                AND RS.EndPeriodDate = @EndDate
                LEFT JOIN TRANSACTION_STATUSES TS ON RS.TransactionStatusId = TS.TransactionStatusId
                LEFT JOIN SubmissionsCounts SC ON CD.ConsultantId = SC.ConsultantId AND P.ProjectId = SC.ProjectId
                LEFT JOIN NumApprovedSubmissionsConsistent NAS ON CD.ConsultantId = NAS.ConsultantId
                LEFT JOIN ActiveProjectsCount APC ON FC.ConsultantId = APC.ConsultantId
                WHERE 
                    (@ProjectId IS NULL OR P.ProjectId = @ProjectId) AND
                    (@TransactionStatusName IS NULL OR 
                    ((@TransactionStatusName = 'Approved' AND PCA.AccessToTrackingTool = 0 AND RS.TransactionStatusId IS NULL) 
                    OR (@TransactionStatusName = 'Pending' AND PCA.AccessToTrackingTool <> 0 AND TS.Name IS NULL)
                    OR TS.Name = @TransactionStatusName)) 
                    AND
                    (@SearchText IS NULL 
                        OR LOWER(U.Name) LIKE '%' + LOWER(@SearchText) + '%' 
                        OR LOWER(U.LastName) LIKE '%' + LOWER(@SearchText) + '%' 
                        OR LOWER(CONCAT(U.Name, ' ', U.LastName)) LIKE '%' + LOWER(@SearchText) + '%') AND
                    (@PaymentPeriod IS NULL OR CD.PaymentPeriod = @PaymentPeriod)
                    AND NOT EXISTS (
                        -- Exclude consultants who have a disabled tracking record within the period
                        SELECT 1
                        FROM PROJECTS_CONSULTANTS_PERIODS_DISABLED_TRACKINGS PCPDT
                        WHERE PCPDT.ProjectId = P.ProjectId
                        AND PCPDT.ConsultantId = CD.ConsultantId
                        AND PCPDT.StartPeriodDate >= @StartDate
                        AND PCPDT.EndPeriodDate <= @EndDate
                        AND PCPDT.EndPeriodDate = @EndDate
                    )
            )
            -- Store results in a temporary table
            SELECT * INTO #TempPagedResults FROM PagedResults WHERE RowNumber = 1;
            
            -- Get the total count
            SELECT @TotalCount = COUNT(*) FROM #TempPagedResults;
            
            -- Retrieve paginated results
            SELECT 
            ConsultantName,
            ConsultantId,
            ProjectId,
            ProjectName,
            SubmissionId,
            TransactionStatusName,
            SubmissionDate,
            LastSubmissionDate,
            NumApprovedSubmissions,
            NumProjectsIsActive,
            AccessToTrackingTool
            FROM #TempPagedResults
            ORDER BY 
                CASE WHEN @FieldToOrder = 'ConsultantName' AND @DirectionOrder = 'ASC' THEN ConsultantName END ASC,
                CASE WHEN @FieldToOrder = 'ConsultantName' AND @DirectionOrder = 'DESC' THEN ConsultantName END DESC,
                CASE WHEN @FieldToOrder = 'ProjectName' AND @DirectionOrder = 'ASC' THEN ProjectName END ASC,
                CASE WHEN @FieldToOrder = 'ProjectName' AND @DirectionOrder = 'DESC' THEN ProjectName END DESC,
                CASE WHEN @FieldToOrder = 'TransactionStatusName' AND @DirectionOrder = 'ASC' THEN TransactionStatusName END ASC,
                CASE WHEN @FieldToOrder = 'TransactionStatusName' AND @DirectionOrder = 'DESC' THEN TransactionStatusName END DESC,
                CASE WHEN @FieldToOrder = 'SubmissionDate' AND @DirectionOrder = 'ASC' THEN SubmissionDate END ASC,
                CASE WHEN @FieldToOrder = 'SubmissionDate' AND @DirectionOrder = 'DESC' THEN SubmissionDate END DESC,
                CASE WHEN @FieldToOrder = 'LastSubmissionDate' AND @DirectionOrder = 'ASC' THEN LastSubmissionDate END ASC,
                CASE WHEN @FieldToOrder = 'LastSubmissionDate' AND @DirectionOrder = 'DESC' THEN LastSubmissionDate END DESC,
                ConsultantName
            OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;
            
            -- Clean up the temporary table
            DROP TABLE #TempPagedResults;
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
            @TransactionStatusName NVARCHAR(80),
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
                AND (SCRMTS.StartPeriodDate >= @StartDate
                OR SCRMTS.EndPeriodDate <= @EndDate)
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
                        -- 1. Consultant Assigned First Time before or on EndDate
                        (HA.Name = 'Consultant Assigned First Time' AND PCAH.ActionDate <= @EndDate) OR
                        -- 2. Consultant Activated before or on EndDate
                        (HA.Name = 'Consultant Activated' AND PCAH.ActionDate <= @EndDate) OR
                        -- 3. Consultant Activated within the period
                        (HA.Name = 'Consultant Activated' AND PCAH.ActionDate >= @StartDate AND PCAH.ActionDate <= @EndDate)
                    )
                    AND NOT EXISTS (
                        -- Exclude consultants whose most recent history before StartDate is 'Consultant Deactivated' 
                        -- and no 'Consultant Activated' action within the period
                        SELECT 1
                        FROM (
                            SELECT TOP 1 HA2.Name, PCAH2.ActionDate
                            FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH2
                            INNER JOIN PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS HA2 ON PCAH2.ActionId = HA2.ActionId
                            WHERE PCAH2.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
                            AND PCAH2.ActionDate < @StartDate
                            ORDER BY PCAH2.ActionDate DESC, PCAH2.Id DESC
                        ) SubQuery
                        WHERE SubQuery.Name = 'Consultant Deactivated'
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
            -- Filtered active consultants
            FilteredConsultants AS (
                SELECT
                    AC.ConsultantId,
                    AC.ProjectId
                FROM ActiveConsultants AC
                JOIN PROJECTS_CONSULTANTS_ASSIGNED PCA ON AC.ConsultantId = PCA.ConsultantId AND AC.ProjectId = PCA.ProjectId
            ),
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
            ),
            -- Main query with pagination and total count
            PagedResults AS (
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
                        ELSE ISNULL(TS.Name, 'Pending') 
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
                    PCA.AccessToTrackingTool,
                    ROW_NUMBER() OVER (PARTITION BY CONCAT(U.Name, ' ', U.LastName), P.Name ORDER BY P.Name) AS RowNumber
                FROM FilteredConsultants FC
                JOIN PROJECTS_CONSULTANTS_ASSIGNED PCA ON FC.ConsultantId = PCA.ConsultantId AND FC.ProjectId = PCA.ProjectId
                JOIN PROJECTS P ON PCA.ProjectId = P.ProjectId
                JOIN CONSULTANT_DETAILS CD ON PCA.ConsultantId = CD.ConsultantId
                JOIN Users U ON CD.UserId = U.Id
                LEFT JOIN REPORTING_MY_TIME_MOVEMENTS_SUBMISSIONS RS ON PCA.ConsultantId = RS.ConsultantId
                AND P.ProjectId = RS.ProjectId AND (RS.StartPeriodDate >= @StartDate OR RS.EndPeriodDate <= @EndDate)
                AND RS.EndPeriodDate = @EndDate
                LEFT JOIN TRANSACTION_STATUSES TS ON RS.TransactionStatusId = TS.TransactionStatusId
                LEFT JOIN SubmissionsCounts SC ON CD.ConsultantId = SC.ConsultantId AND P.ProjectId = SC.ProjectId
                LEFT JOIN NumApprovedSubmissionsConsistent NAS ON CD.ConsultantId = NAS.ConsultantId
                LEFT JOIN ActiveProjectsCount APC ON FC.ConsultantId = APC.ConsultantId
                WHERE 
                    (@ProjectId IS NULL OR P.ProjectId = @ProjectId) AND
                    (@TransactionStatusName IS NULL OR 
                    ((@TransactionStatusName = 'Approved' AND PCA.AccessToTrackingTool = 0 AND RS.TransactionStatusId IS NULL) 
                    OR (@TransactionStatusName = 'Pending' AND PCA.AccessToTrackingTool <> 0 AND TS.Name IS NULL)
                    OR TS.Name = @TransactionStatusName)) 
                    AND
                    (@SearchText IS NULL 
                        OR LOWER(U.Name) LIKE '%' + LOWER(@SearchText) + '%' 
                        OR LOWER(U.LastName) LIKE '%' + LOWER(@SearchText) + '%' 
                        OR LOWER(CONCAT(U.Name, ' ', U.LastName)) LIKE '%' + LOWER(@SearchText) + '%') AND
                    (@PaymentPeriod IS NULL OR CD.PaymentPeriod = @PaymentPeriod)
            )
            -- Store results in a temporary table
            SELECT * INTO #TempPagedResults FROM PagedResults WHERE RowNumber = 1;
            
            -- Get the total count
            SELECT @TotalCount = COUNT(*) FROM #TempPagedResults;
            
            -- Retrieve paginated results
            SELECT *
            FROM #TempPagedResults
            ORDER BY 
                CASE WHEN @FieldToOrder = 'ConsultantName' AND @DirectionOrder = 'ASC' THEN ConsultantName END ASC,
                CASE WHEN @FieldToOrder = 'ConsultantName' AND @DirectionOrder = 'DESC' THEN ConsultantName END DESC,
                CASE WHEN @FieldToOrder = 'ProjectName' AND @DirectionOrder = 'ASC' THEN ProjectName END ASC,
                CASE WHEN @FieldToOrder = 'ProjectName' AND @DirectionOrder = 'DESC' THEN ProjectName END DESC,
                CASE WHEN @FieldToOrder = 'TransactionStatusName' AND @DirectionOrder = 'ASC' THEN TransactionStatusName END ASC,
                CASE WHEN @FieldToOrder = 'TransactionStatusName' AND @DirectionOrder = 'DESC' THEN TransactionStatusName END DESC,
                CASE WHEN @FieldToOrder = 'SubmissionDate' AND @DirectionOrder = 'ASC' THEN SubmissionDate END ASC,
                CASE WHEN @FieldToOrder = 'SubmissionDate' AND @DirectionOrder = 'DESC' THEN SubmissionDate END DESC,
                CASE WHEN @FieldToOrder = 'LastSubmissionDate' AND @DirectionOrder = 'ASC' THEN LastSubmissionDate END ASC,
                CASE WHEN @FieldToOrder = 'LastSubmissionDate' AND @DirectionOrder = 'DESC' THEN LastSubmissionDate END DESC,
                ConsultantName
            OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;
            
            -- Clean up the temporary table
            DROP TABLE #TempPagedResults;
            END";

            migrationBuilder.Sql("DROP PROCEDURE IF SP_PAYMENT_SHEETS_GetAllConsultantsToPayWithFilters");
            migrationBuilder.Sql(spOriginal);
        }
    }
}
