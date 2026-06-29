using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class thirtysixFixSP_PAYMENT_SHEETS_ProjectLessDisabledTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Bugfix: a consultant removed from their only project via the payment-sheet
            // "remove for this period" action (which writes PROJECTS_CONSULTANTS_PERIODS_DISABLED_TRACKINGS)
            // disappeared from the sheet entirely, even with a non-rejected interview in the period.
            // The project row was correctly suppressed, but ActiveConsultants still counted the
            // period-disabled assignment, which blocked the project-less interview fallback.
            // Fix: in ProjectLessConsultants, ignore active assignments that are period-disabled
            // for this period (same predicate already used by PagedResults / ActiveProjectsCount).
            var sp = @"CREATE PROCEDURE SP_PAYMENT_SHEETS_GetAllConsultantsToPayWithFilters
            @SearchText NVARCHAR(100),
            @StartDate DATE,
            @EndDate DATE,
            @TransactionStatusName VARCHAR(80),
            @AccountsPayableStatusName VARCHAR(30),
            @ProjectIds dbo.ConsultantProjectIdTableType READONLY,
            @PaymentPeriod INT,
            @FieldToOrder VARCHAR(60),
            @DirectionOrder VARCHAR(60),
            @Skip INT,
            @Take INT,
            @TotalCount INT OUTPUT
            AS
            BEGIN
            SET NOCOUNT ON;
            -- Subquery to determine the correct payment status for each consultant
            WITH AccountsPayableStatus AS (
                SELECT
                    C.ConsultantId,
                    -- Determine the correct PaymentStatus based on the records found
                    CASE
                        -- If there are no records in ACCOUNTS_PAYABLE (after filtering Voided = 0), return 'Pending'
                        WHEN COUNT(AP.ConsultantId) = 0 THEN 'Pending'
                        -- If at least one record has status 'Updated - Pending Review', return that status
                        WHEN SUM(CASE WHEN TS.Name = 'Updated - Pending Review' THEN 1 ELSE 0 END) > 0 THEN 'Updated - Pending Review'
                        -- If no 'Updated - Pending Review' but at least one 'Sent to be paid', return 'Sent to be paid'
                        WHEN SUM(CASE WHEN TS.Name = 'Sent to be paid' THEN 1 ELSE 0 END) > 0 THEN 'Sent to be paid'
                        -- If all statuses are the same, return that status
                        WHEN COUNT(DISTINCT TS.Name) = 1 THEN MAX(TS.Name)
                        -- Otherwise, return the most common status
                        ELSE MAX(TS.Name)
                    END AS PaymentStatus
                FROM CONSULTANT_DETAILS C
                LEFT JOIN ACCOUNTS_PAYABLE AP ON C.ConsultantId = AP.ConsultantId
                LEFT JOIN TRANSACTION_STATUSES TS ON AP.TransactionStatusId = TS.TransactionStatusId
                WHERE AP.Voided = 0  -- Only consider non-voided records
                  AND (AP.StartDatePeriod >= @StartDate OR AP.StartDatePeriod IS NULL) -- Handle cases with no AP records
                  AND (AP.EndDatePeriod <= @EndDate OR AP.EndDatePeriod IS NULL) -- Handle cases with no AP records
                GROUP BY C.ConsultantId
            ),
            -- Subquery to count submissions for consultants within the period
            SubmissionsCounts AS (
                SELECT
                    ConsultantId,
                    ProjectId,
                    COUNT(*) AS NumSubmissions
                FROM REPORTING_MY_TIME_MOVEMENTS_SUBMISSIONS SCRMTS
                INNER JOIN TRANSACTION_STATUSES SCTS ON SCRMTS.TransactionStatusId = SCTS.TransactionStatusId
                WHERE SCTS.Name = 'Approved'
                AND (SCRMTS.StartPeriodDate >= @StartDate OR SCRMTS.EndPeriodDate <= @EndDate)
                AND SCRMTS.EndPeriodDate = @EndDate
                GROUP BY ConsultantId, ProjectId
            ),

            -- Determine active consultants based on activity filters
            IsActiveStatus AS (
                SELECT
                    PCA.ProjectConsultantAssignedId,
                    CASE
                        -- Check most recent record before @StartDate
                        WHEN EXISTS (
                            SELECT 1
                            FROM (
                                SELECT TOP 1 PCAH.IsActive, PCAH.MonthlySalary, PCAH.HourlySalary
                                FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH
                                WHERE PCAH.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
                                AND PCAH.ActionDate < @StartDate
                                ORDER BY PCAH.ActionDate DESC, PCAH.Id DESC
                            ) AS LastRecordBeforeStartDate
                            WHERE LastRecordBeforeStartDate.IsActive = 1
                            AND (LastRecordBeforeStartDate.MonthlySalary > 0 OR LastRecordBeforeStartDate.HourlySalary > 0)
                        ) THEN 1
                        -- If there are no records before @StartDate, check records between @StartDate and @EndDate
                        WHEN EXISTS (
                            SELECT 1
                            FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH
                            WHERE PCAH.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
                            AND PCAH.ActionDate >= @StartDate
                            AND PCAH.ActionDate <= @EndDate
                            AND PCAH.IsActive = 1
                            AND (PCAH.MonthlySalary > 0 OR PCAH.HourlySalary > 0)
                        ) THEN 1
                        ELSE 0
                    END AS IsActive
                FROM PROJECTS_CONSULTANTS_ASSIGNED PCA
            )
            ,

            -- Filter active consultants based on IsActive status
            ActiveConsultants AS (
                SELECT
                    PCA.ConsultantId,
                    PCA.ProjectId,
                    PCA.ProjectConsultantAssignedId
                FROM PROJECTS_CONSULTANTS_ASSIGNED PCA
                JOIN IsActiveStatus IAS ON PCA.ProjectConsultantAssignedId = IAS.ProjectConsultantAssignedId
                JOIN PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH
                    ON PCA.ProjectConsultantAssignedId = PCAH.ProjectConsultantAssignedId
                WHERE IAS.IsActive = 1 -- Only consider consultants that are active

            ),

            -- Determine AccessToTrackingTool
            AccessToTrackingToolData AS (
                SELECT
                    PCA.ProjectConsultantAssignedId,
                    ISNULL((SELECT TOP 1 PCAH.AccessToTrackingTool
                            FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH
                            WHERE PCAH.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
                            AND PCAH.ActionDate <= @EndDate
                            ORDER BY PCAH.ActionDate DESC, PCAH.Id DESC), 0) AS AccessToTrackingTool
                FROM PROJECTS_CONSULTANTS_ASSIGNED PCA
            ),

            -- Calculate the number of approved submissions for each consultant
            NumApprovedSubmissionsData AS (
                SELECT
                    CD.ConsultantId,
                    P.ProjectId,
                    COALESCE(SC.NumSubmissions, 0) AS NumSubmissions,
                    ATTD.AccessToTrackingTool
                FROM PROJECTS_CONSULTANTS_ASSIGNED PCA
                JOIN CONSULTANT_DETAILS CD ON PCA.ConsultantId = CD.ConsultantId
                JOIN PROJECTS P ON PCA.ProjectId = P.ProjectId
                LEFT JOIN SubmissionsCounts SC ON CD.ConsultantId = SC.ConsultantId AND P.ProjectId = SC.ProjectId
                LEFT JOIN AccessToTrackingToolData ATTD ON PCA.ProjectConsultantAssignedId = ATTD.ProjectConsultantAssignedId
                WHERE EXISTS (
                    SELECT 1
                    FROM ActiveConsultants AC
                    WHERE AC.ConsultantId = PCA.ConsultantId
                    AND AC.ProjectId = PCA.ProjectId
                )
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
            ),

            NumApprovedSubmissionsConsistent AS (
                SELECT
                    ConsultantId,
                    SUM(CASE
                        WHEN NumSubmissions > 0 THEN 1
                        WHEN AccessToTrackingTool = 0 THEN 1
                        ELSE 0
                    END) AS NumApprovedSubmissions
                FROM NumApprovedSubmissionsData
                GROUP BY ConsultantId
            ),

            -- Calculate the number of active projects for each consultant
            ActiveProjectsCount AS (
                SELECT
                    FC.ConsultantId,
                    COUNT(DISTINCT FC.ProjectId) AS NumProjectsIsActive
                FROM ActiveConsultants FC
                LEFT JOIN PROJECTS_CONSULTANTS_PERIODS_DISABLED_TRACKINGS PCPDT
                    ON FC.ProjectId = PCPDT.ProjectId
                    AND FC.ConsultantId = PCPDT.ConsultantId
                    AND PCPDT.StartPeriodDate >= @StartDate
                    AND PCPDT.EndPeriodDate <= @EndDate
                    AND PCPDT.EndPeriodDate = @EndDate
                WHERE PCPDT.ProjectId IS NULL -- Exclude projects that appear in the disabled tracking table
                GROUP BY FC.ConsultantId
            ),
            MovementsInPeriod AS (
                SELECT
                    M.MovementId,
                    M.ConsultantId,
                    M.ProjectId,
                    M.MovementTypeId,
                    M.IsBillable,
                    CAST(COALESCE(M.Quantity, 0) AS decimal(18,2)) AS Quantity
                FROM REPORTING_MY_TIME_MOVEMENTS M
                WHERE M.ActionDate >= @StartDate
                  AND M.ActionDate <= @EndDate
            ),
            ProfileImages AS (
                SELECT
                    IB.EntityId,
                    IB.BlobUrl,
                    ROW_NUMBER() OVER (
                        PARTITION BY IB.EntityId
                        ORDER BY IB.CreationDate DESC, IB.BlobId DESC
                    ) AS rn
                FROM IMAGE_BLOBS IB
                WHERE IB.EntityType = 'UserProfile'
                  AND IB.BlobUrl IS NOT NULL
            ),
            -- Project-less consultants: a non-rejected INTERVIEW, debit/credit, OR reimbursement in
            -- the period and ZERO active assignments. Each surfaces as a single project-less row
            -- (one row per consultant; mixed categories do not duplicate). NHP-2.1 added the
            -- debit/credit and reimbursement arms to the M1 interview-only predicate.
            ProjectLessConsultants AS (
                SELECT DISTINCT CD.ConsultantId
                FROM CONSULTANT_DETAILS CD
                WHERE (
                    EXISTS (
                        SELECT 1
                        FROM INTERVIEWS I
                        INNER JOIN TRANSACTION_STATUSES ITS ON I.TransactionStatusId = ITS.TransactionStatusId
                        WHERE I.ConsultantId = CD.ConsultantId
                          AND I.Date >= @StartDate
                          AND I.Date <= @EndDate
                          AND ITS.Name <> 'Rejected'
                    )
                    OR EXISTS (
                        SELECT 1
                        FROM CONSULTANT_PAYMENTS_DEBITS_CREDITS DC
                        INNER JOIN TRANSACTION_STATUSES DCTS ON DC.TransactionStatusId = DCTS.TransactionStatusId
                        WHERE DC.ConsultantId = CD.ConsultantId
                          AND DC.ActionDateWithinFortnight >= @StartDate
                          AND DC.ActionDateWithinFortnight <= @EndDate
                          AND DCTS.Name <> 'Rejected'
                    )
                    OR EXISTS (
                        SELECT 1
                        FROM CONSULTANT_REIMBURSED_BENEFITS RB
                        INNER JOIN TRANSACTION_STATUSES RBTS ON RB.TransactionStatusId = RBTS.TransactionStatusId
                        WHERE RB.ConsultantId = CD.ConsultantId
                          AND RB.DateToBeReimbursed >= @StartDate
                          AND RB.DateToBeReimbursed <= @EndDate
                          AND RBTS.Name <> 'Rejected'
                    )
                )
                AND NOT EXISTS (
                    SELECT 1 FROM ActiveConsultants AC WHERE AC.ConsultantId = CD.ConsultantId
                      AND NOT EXISTS (
                          -- Treat a project whose period was disabled (payment-sheet removal) as NOT active,
                          -- so a consultant whose only assignment is period-disabled can surface project-less.
                          SELECT 1 FROM PROJECTS_CONSULTANTS_PERIODS_DISABLED_TRACKINGS PCPDT
                          WHERE PCPDT.ProjectId = AC.ProjectId
                            AND PCPDT.ConsultantId = AC.ConsultantId
                            AND PCPDT.StartPeriodDate >= @StartDate
                            AND PCPDT.EndPeriodDate <= @EndDate
                            AND PCPDT.EndPeriodDate = @EndDate
                      )
                )
            ),
            -- Main query with the correct PaymentStatus and paginated results
            PagedResults AS (
                SELECT
                    CONCAT(U.Name, ' ', U.LastName) AS ConsultantName,
                    CD.ConsultantId,
                    P.ProjectId,
                    P.Name AS ProjectName,
                    CASE
                        WHEN ATTD.AccessToTrackingTool = 0 AND RS.SubmissionId IS NULL THEN NULL
                        ELSE RS.SubmissionId
                    END AS SubmissionId,
                    CASE
                        WHEN ATTD.AccessToTrackingTool = 0 AND RS.TransactionStatusId IS NULL THEN 'Approved'
                        ELSE ISNULL(TS.Name, 'Pending')
                    END AS TransactionStatusName,
                    CASE
                        WHEN ATTD.AccessToTrackingTool = 0 AND RS.SubmissionDate IS NULL THEN NULL
                        ELSE RS.SubmissionDate
                    END AS SubmissionDate,
                    CASE
                        WHEN ATTD.AccessToTrackingTool = 0 AND RS.LastSubmissionDate IS NULL THEN NULL
                        ELSE RS.LastSubmissionDate
                    END AS LastSubmissionDate,
                    NAS.NumApprovedSubmissions,
                    APC.NumProjectsIsActive,
                    ATTD.AccessToTrackingTool,
                    COALESCE(APS.PaymentStatus, 'Pending') AS PaymentStatus, -- If no records in ACCOUNTS_PAYABLE, show 'Pending'
                    ISNULL(JSON_QUERY((
                        SELECT MT.Name AS MovementType,
                               SUM(MIP.Quantity) AS Quantity,
                               CAST(MIP.IsBillable AS bit) AS IsBillable
                        FROM MovementsInPeriod MIP
                        JOIN REPORTING_MY_TIME_MOVEMENT_TYPES MT ON MT.MovementTypeId = MIP.MovementTypeId
                        WHERE MIP.ConsultantId = CD.ConsultantId
                          AND MIP.ProjectId    = P.ProjectId
                        GROUP BY MT.Name, MIP.IsBillable
                        FOR JSON PATH
                    )), '[]') AS HoursReported,
                           ISNULL(JSON_QUERY((
                        SELECT COALESCE(B.PrimaryReportTrackingToolName, B.SecondReportTrackingToolName) AS TrackingToolName,
                               B.BlobUrl
                        FROM MovementsInPeriod MIP
                        JOIN REPORTING_MY_TIME_MOVEMENT_BLOBS B ON B.MovementId = MIP.MovementId
                        WHERE MIP.ConsultantId = CD.ConsultantId
                          AND MIP.ProjectId    = P.ProjectId
                        FOR JSON PATH
                    )), '[]') AS ReportAttachments,
                    PI.BlobUrl AS ProfileImage,
                    ROW_NUMBER() OVER (PARTITION BY CONCAT(U.Name, ' ', U.LastName), P.Name ORDER BY P.Name) AS RowNumber
                FROM ActiveConsultants FC
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
                LEFT JOIN AccessToTrackingToolData ATTD ON PCA.ProjectConsultantAssignedId = ATTD.ProjectConsultantAssignedId
                LEFT JOIN AccountsPayableStatus APS ON CD.ConsultantId = APS.ConsultantId
                LEFT JOIN ProfileImages PI ON PI.EntityId = CD.UserId AND PI.rn = 1
                WHERE
                    (NOT EXISTS (SELECT 1 FROM @ProjectIds)
                    OR P.ProjectId IN (SELECT ConsultantProjectId FROM @ProjectIds)) AND
                    (@TransactionStatusName IS NULL OR
                    ((@TransactionStatusName = 'Approved' AND ATTD.AccessToTrackingTool = 0 AND RS.TransactionStatusId IS NULL)
                    OR (@TransactionStatusName = 'Pending' AND ATTD.AccessToTrackingTool <> 0 AND TS.Name IS NULL)
                    OR TS.Name = @TransactionStatusName))
                    AND
                    (@SearchText IS NULL
                        OR LOWER(U.Name) LIKE '%' + LOWER(@SearchText) + '%'
                        OR LOWER(U.LastName) LIKE '%' + LOWER(@SearchText) + '%'
                        OR LOWER(CONCAT(U.Name, ' ', U.LastName)) LIKE '%' + LOWER(@SearchText) + '%') AND
                    (@PaymentPeriod IS NULL OR CD.PaymentPeriod = @PaymentPeriod)
            		AND (@AccountsPayableStatusName IS NULL OR APS.PaymentStatus = @AccountsPayableStatusName OR
                    (@AccountsPayableStatusName = 'Pending' AND APS.PaymentStatus IS NULL))  -- Add the filter by PaymentStatus
                    AND NOT EXISTS (
                        -- Exclude consultants with disabled tracking
                        SELECT 1
                        FROM PROJECTS_CONSULTANTS_PERIODS_DISABLED_TRACKINGS PCPDT
                        WHERE PCPDT.ProjectId = P.ProjectId
                        AND PCPDT.ConsultantId = CD.ConsultantId
                        AND PCPDT.StartPeriodDate >= @StartDate
                        AND PCPDT.EndPeriodDate <= @EndDate
                        AND PCPDT.EndPeriodDate = @EndDate
                    )
            ),
            -- Project-less rows: one row per project-less consultant ('—', hours 0).
            ProjectLessPagedResults AS (
                SELECT
                    CONCAT(U.Name, ' ', U.LastName) AS ConsultantName,
                    CD.ConsultantId,
                    CAST(NULL AS INT) AS ProjectId,
                    N'—' AS ProjectName,
                    CAST(NULL AS INT) AS SubmissionId,
                    'Approved' AS TransactionStatusName,
                    CAST(NULL AS DATETIME2) AS SubmissionDate,
                    CAST(NULL AS DATETIME2) AS LastSubmissionDate,
                    0 AS NumApprovedSubmissions,
                    0 AS NumProjectsIsActive,
                    CAST(0 AS BIT) AS AccessToTrackingTool,
                    COALESCE(APS.PaymentStatus, 'Pending') AS PaymentStatus,
                    CAST('[]' AS NVARCHAR(MAX)) AS HoursReported,
                    CAST('[]' AS NVARCHAR(MAX)) AS ReportAttachments,
                    PI.BlobUrl AS ProfileImage
                FROM ProjectLessConsultants PLC
                JOIN CONSULTANT_DETAILS CD ON PLC.ConsultantId = CD.ConsultantId
                JOIN Users U ON CD.UserId = U.Id
                LEFT JOIN AccountsPayableStatus APS ON CD.ConsultantId = APS.ConsultantId
                LEFT JOIN ProfileImages PI ON PI.EntityId = CD.UserId AND PI.rn = 1
                WHERE
                    -- Drop project-less rows when a project filter is applied
                    NOT EXISTS (SELECT 1 FROM @ProjectIds)
                    AND (@TransactionStatusName IS NULL OR @TransactionStatusName = 'Approved')
                    AND (@SearchText IS NULL
                        OR LOWER(U.Name) LIKE '%' + LOWER(@SearchText) + '%'
                        OR LOWER(U.LastName) LIKE '%' + LOWER(@SearchText) + '%'
                        OR LOWER(CONCAT(U.Name, ' ', U.LastName)) LIKE '%' + LOWER(@SearchText) + '%')
                    AND (@PaymentPeriod IS NULL OR CD.PaymentPeriod = @PaymentPeriod)
                    AND (@AccountsPayableStatusName IS NULL OR COALESCE(APS.PaymentStatus, 'Pending') = @AccountsPayableStatusName OR
                        (@AccountsPayableStatusName = 'Pending' AND APS.PaymentStatus IS NULL))
            )

            -- Store results in a temporary table for further processing
            SELECT *
            INTO #TempPagedResults
            FROM (
                SELECT
                    ConsultantName, ConsultantId, ProjectId, ProjectName, SubmissionId, TransactionStatusName,
                    SubmissionDate, LastSubmissionDate, NumApprovedSubmissions, NumProjectsIsActive,
                    AccessToTrackingTool, PaymentStatus, HoursReported, ReportAttachments, ProfileImage
                FROM PagedResults
                WHERE RowNumber = 1
                UNION ALL
                SELECT
                    ConsultantName, ConsultantId, ProjectId, ProjectName, SubmissionId, TransactionStatusName,
                    SubmissionDate, LastSubmissionDate, NumApprovedSubmissions, NumProjectsIsActive,
                    AccessToTrackingTool, PaymentStatus, HoursReported, ReportAttachments, ProfileImage
                FROM ProjectLessPagedResults
            ) AS Combined;

            -- Get the total count for pagination
            SELECT @TotalCount = COUNT(*) FROM #TempPagedResults;

            -- Fetch the paginated results
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
                AccessToTrackingTool,
                PaymentStatus,
                REPLACE(ReportAttachments, '\/', '/') AS ReportAttachments,
                REPLACE(HoursReported, '\/', '/')      AS HoursReported,
                ProfileImage
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
            	CASE WHEN @FieldToOrder = 'PaymentStatus' AND @DirectionOrder = 'ASC' THEN PaymentStatus END ASC,
                CASE WHEN @FieldToOrder = 'PaymentStatus' AND @DirectionOrder = 'DESC' THEN PaymentStatus END DESC,
                ConsultantName
            OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;

            -- Drop the temporary table
            DROP TABLE #TempPagedResults;
            END";

            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS SP_PAYMENT_SHEETS_GetAllConsultantsToPayWithFilters");
            migrationBuilder.Sql(sp);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Restore the pre-fix procedure (project-less fallback blocked by any active assignment,
            // including period-disabled ones).
            var sp = @"CREATE PROCEDURE SP_PAYMENT_SHEETS_GetAllConsultantsToPayWithFilters
            @SearchText NVARCHAR(100),
            @StartDate DATE,
            @EndDate DATE,
            @TransactionStatusName VARCHAR(80),
            @AccountsPayableStatusName VARCHAR(30),
            @ProjectIds dbo.ConsultantProjectIdTableType READONLY,
            @PaymentPeriod INT,
            @FieldToOrder VARCHAR(60),
            @DirectionOrder VARCHAR(60),
            @Skip INT,
            @Take INT,
            @TotalCount INT OUTPUT
            AS
            BEGIN
            SET NOCOUNT ON;
            -- Subquery to determine the correct payment status for each consultant
            WITH AccountsPayableStatus AS (
                SELECT
                    C.ConsultantId,
                    -- Determine the correct PaymentStatus based on the records found
                    CASE
                        -- If there are no records in ACCOUNTS_PAYABLE (after filtering Voided = 0), return 'Pending'
                        WHEN COUNT(AP.ConsultantId) = 0 THEN 'Pending'
                        -- If at least one record has status 'Updated - Pending Review', return that status
                        WHEN SUM(CASE WHEN TS.Name = 'Updated - Pending Review' THEN 1 ELSE 0 END) > 0 THEN 'Updated - Pending Review'
                        -- If no 'Updated - Pending Review' but at least one 'Sent to be paid', return 'Sent to be paid'
                        WHEN SUM(CASE WHEN TS.Name = 'Sent to be paid' THEN 1 ELSE 0 END) > 0 THEN 'Sent to be paid'
                        -- If all statuses are the same, return that status
                        WHEN COUNT(DISTINCT TS.Name) = 1 THEN MAX(TS.Name)
                        -- Otherwise, return the most common status
                        ELSE MAX(TS.Name)
                    END AS PaymentStatus
                FROM CONSULTANT_DETAILS C
                LEFT JOIN ACCOUNTS_PAYABLE AP ON C.ConsultantId = AP.ConsultantId
                LEFT JOIN TRANSACTION_STATUSES TS ON AP.TransactionStatusId = TS.TransactionStatusId
                WHERE AP.Voided = 0  -- Only consider non-voided records
                  AND (AP.StartDatePeriod >= @StartDate OR AP.StartDatePeriod IS NULL) -- Handle cases with no AP records
                  AND (AP.EndDatePeriod <= @EndDate OR AP.EndDatePeriod IS NULL) -- Handle cases with no AP records
                GROUP BY C.ConsultantId
            ),
            -- Subquery to count submissions for consultants within the period
            SubmissionsCounts AS (
                SELECT
                    ConsultantId,
                    ProjectId,
                    COUNT(*) AS NumSubmissions
                FROM REPORTING_MY_TIME_MOVEMENTS_SUBMISSIONS SCRMTS
                INNER JOIN TRANSACTION_STATUSES SCTS ON SCRMTS.TransactionStatusId = SCTS.TransactionStatusId
                WHERE SCTS.Name = 'Approved'
                AND (SCRMTS.StartPeriodDate >= @StartDate OR SCRMTS.EndPeriodDate <= @EndDate)
                AND SCRMTS.EndPeriodDate = @EndDate
                GROUP BY ConsultantId, ProjectId
            ),

            -- Determine active consultants based on activity filters
            IsActiveStatus AS (
                SELECT
                    PCA.ProjectConsultantAssignedId,
                    CASE
                        -- Check most recent record before @StartDate
                        WHEN EXISTS (
                            SELECT 1
                            FROM (
                                SELECT TOP 1 PCAH.IsActive, PCAH.MonthlySalary, PCAH.HourlySalary
                                FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH
                                WHERE PCAH.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
                                AND PCAH.ActionDate < @StartDate
                                ORDER BY PCAH.ActionDate DESC, PCAH.Id DESC
                            ) AS LastRecordBeforeStartDate
                            WHERE LastRecordBeforeStartDate.IsActive = 1
                            AND (LastRecordBeforeStartDate.MonthlySalary > 0 OR LastRecordBeforeStartDate.HourlySalary > 0)
                        ) THEN 1
                        -- If there are no records before @StartDate, check records between @StartDate and @EndDate
                        WHEN EXISTS (
                            SELECT 1
                            FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH
                            WHERE PCAH.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
                            AND PCAH.ActionDate >= @StartDate
                            AND PCAH.ActionDate <= @EndDate
                            AND PCAH.IsActive = 1
                            AND (PCAH.MonthlySalary > 0 OR PCAH.HourlySalary > 0)
                        ) THEN 1
                        ELSE 0
                    END AS IsActive
                FROM PROJECTS_CONSULTANTS_ASSIGNED PCA
            )
            ,

            -- Filter active consultants based on IsActive status
            ActiveConsultants AS (
                SELECT
                    PCA.ConsultantId,
                    PCA.ProjectId,
                    PCA.ProjectConsultantAssignedId
                FROM PROJECTS_CONSULTANTS_ASSIGNED PCA
                JOIN IsActiveStatus IAS ON PCA.ProjectConsultantAssignedId = IAS.ProjectConsultantAssignedId
                JOIN PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH
                    ON PCA.ProjectConsultantAssignedId = PCAH.ProjectConsultantAssignedId
                WHERE IAS.IsActive = 1 -- Only consider consultants that are active

            ),

            -- Determine AccessToTrackingTool
            AccessToTrackingToolData AS (
                SELECT
                    PCA.ProjectConsultantAssignedId,
                    ISNULL((SELECT TOP 1 PCAH.AccessToTrackingTool
                            FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH
                            WHERE PCAH.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
                            AND PCAH.ActionDate <= @EndDate
                            ORDER BY PCAH.ActionDate DESC, PCAH.Id DESC), 0) AS AccessToTrackingTool
                FROM PROJECTS_CONSULTANTS_ASSIGNED PCA
            ),

            -- Calculate the number of approved submissions for each consultant
            NumApprovedSubmissionsData AS (
                SELECT
                    CD.ConsultantId,
                    P.ProjectId,
                    COALESCE(SC.NumSubmissions, 0) AS NumSubmissions,
                    ATTD.AccessToTrackingTool
                FROM PROJECTS_CONSULTANTS_ASSIGNED PCA
                JOIN CONSULTANT_DETAILS CD ON PCA.ConsultantId = CD.ConsultantId
                JOIN PROJECTS P ON PCA.ProjectId = P.ProjectId
                LEFT JOIN SubmissionsCounts SC ON CD.ConsultantId = SC.ConsultantId AND P.ProjectId = SC.ProjectId
                LEFT JOIN AccessToTrackingToolData ATTD ON PCA.ProjectConsultantAssignedId = ATTD.ProjectConsultantAssignedId
                WHERE EXISTS (
                    SELECT 1
                    FROM ActiveConsultants AC
                    WHERE AC.ConsultantId = PCA.ConsultantId
                    AND AC.ProjectId = PCA.ProjectId
                )
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
            ),

            NumApprovedSubmissionsConsistent AS (
                SELECT
                    ConsultantId,
                    SUM(CASE
                        WHEN NumSubmissions > 0 THEN 1
                        WHEN AccessToTrackingTool = 0 THEN 1
                        ELSE 0
                    END) AS NumApprovedSubmissions
                FROM NumApprovedSubmissionsData
                GROUP BY ConsultantId
            ),

            -- Calculate the number of active projects for each consultant
            ActiveProjectsCount AS (
                SELECT
                    FC.ConsultantId,
                    COUNT(DISTINCT FC.ProjectId) AS NumProjectsIsActive
                FROM ActiveConsultants FC
                LEFT JOIN PROJECTS_CONSULTANTS_PERIODS_DISABLED_TRACKINGS PCPDT
                    ON FC.ProjectId = PCPDT.ProjectId
                    AND FC.ConsultantId = PCPDT.ConsultantId
                    AND PCPDT.StartPeriodDate >= @StartDate
                    AND PCPDT.EndPeriodDate <= @EndDate
                    AND PCPDT.EndPeriodDate = @EndDate
                WHERE PCPDT.ProjectId IS NULL -- Exclude projects that appear in the disabled tracking table
                GROUP BY FC.ConsultantId
            ),
            MovementsInPeriod AS (
                SELECT
                    M.MovementId,
                    M.ConsultantId,
                    M.ProjectId,
                    M.MovementTypeId,
                    M.IsBillable,
                    CAST(COALESCE(M.Quantity, 0) AS decimal(18,2)) AS Quantity
                FROM REPORTING_MY_TIME_MOVEMENTS M
                WHERE M.ActionDate >= @StartDate
                  AND M.ActionDate <= @EndDate
            ),
            ProfileImages AS (
                SELECT
                    IB.EntityId,
                    IB.BlobUrl,
                    ROW_NUMBER() OVER (
                        PARTITION BY IB.EntityId
                        ORDER BY IB.CreationDate DESC, IB.BlobId DESC
                    ) AS rn
                FROM IMAGE_BLOBS IB
                WHERE IB.EntityType = 'UserProfile'
                  AND IB.BlobUrl IS NOT NULL
            ),
            -- Project-less consultants: a non-rejected INTERVIEW, debit/credit, OR reimbursement in
            -- the period and ZERO active assignments. Each surfaces as a single project-less row
            -- (one row per consultant; mixed categories do not duplicate). NHP-2.1 added the
            -- debit/credit and reimbursement arms to the M1 interview-only predicate.
            ProjectLessConsultants AS (
                SELECT DISTINCT CD.ConsultantId
                FROM CONSULTANT_DETAILS CD
                WHERE (
                    EXISTS (
                        SELECT 1
                        FROM INTERVIEWS I
                        INNER JOIN TRANSACTION_STATUSES ITS ON I.TransactionStatusId = ITS.TransactionStatusId
                        WHERE I.ConsultantId = CD.ConsultantId
                          AND I.Date >= @StartDate
                          AND I.Date <= @EndDate
                          AND ITS.Name <> 'Rejected'
                    )
                    OR EXISTS (
                        SELECT 1
                        FROM CONSULTANT_PAYMENTS_DEBITS_CREDITS DC
                        INNER JOIN TRANSACTION_STATUSES DCTS ON DC.TransactionStatusId = DCTS.TransactionStatusId
                        WHERE DC.ConsultantId = CD.ConsultantId
                          AND DC.ActionDateWithinFortnight >= @StartDate
                          AND DC.ActionDateWithinFortnight <= @EndDate
                          AND DCTS.Name <> 'Rejected'
                    )
                    OR EXISTS (
                        SELECT 1
                        FROM CONSULTANT_REIMBURSED_BENEFITS RB
                        INNER JOIN TRANSACTION_STATUSES RBTS ON RB.TransactionStatusId = RBTS.TransactionStatusId
                        WHERE RB.ConsultantId = CD.ConsultantId
                          AND RB.DateToBeReimbursed >= @StartDate
                          AND RB.DateToBeReimbursed <= @EndDate
                          AND RBTS.Name <> 'Rejected'
                    )
                )
                AND NOT EXISTS (
                    SELECT 1 FROM ActiveConsultants AC WHERE AC.ConsultantId = CD.ConsultantId
                )
            ),
            -- Main query with the correct PaymentStatus and paginated results
            PagedResults AS (
                SELECT
                    CONCAT(U.Name, ' ', U.LastName) AS ConsultantName,
                    CD.ConsultantId,
                    P.ProjectId,
                    P.Name AS ProjectName,
                    CASE
                        WHEN ATTD.AccessToTrackingTool = 0 AND RS.SubmissionId IS NULL THEN NULL
                        ELSE RS.SubmissionId
                    END AS SubmissionId,
                    CASE
                        WHEN ATTD.AccessToTrackingTool = 0 AND RS.TransactionStatusId IS NULL THEN 'Approved'
                        ELSE ISNULL(TS.Name, 'Pending')
                    END AS TransactionStatusName,
                    CASE
                        WHEN ATTD.AccessToTrackingTool = 0 AND RS.SubmissionDate IS NULL THEN NULL
                        ELSE RS.SubmissionDate
                    END AS SubmissionDate,
                    CASE
                        WHEN ATTD.AccessToTrackingTool = 0 AND RS.LastSubmissionDate IS NULL THEN NULL
                        ELSE RS.LastSubmissionDate
                    END AS LastSubmissionDate,
                    NAS.NumApprovedSubmissions,
                    APC.NumProjectsIsActive,
                    ATTD.AccessToTrackingTool,
                    COALESCE(APS.PaymentStatus, 'Pending') AS PaymentStatus, -- If no records in ACCOUNTS_PAYABLE, show 'Pending'
                    ISNULL(JSON_QUERY((
                        SELECT MT.Name AS MovementType,
                               SUM(MIP.Quantity) AS Quantity,
                               CAST(MIP.IsBillable AS bit) AS IsBillable
                        FROM MovementsInPeriod MIP
                        JOIN REPORTING_MY_TIME_MOVEMENT_TYPES MT ON MT.MovementTypeId = MIP.MovementTypeId
                        WHERE MIP.ConsultantId = CD.ConsultantId
                          AND MIP.ProjectId    = P.ProjectId
                        GROUP BY MT.Name, MIP.IsBillable
                        FOR JSON PATH
                    )), '[]') AS HoursReported,
                           ISNULL(JSON_QUERY((
                        SELECT COALESCE(B.PrimaryReportTrackingToolName, B.SecondReportTrackingToolName) AS TrackingToolName,
                               B.BlobUrl
                        FROM MovementsInPeriod MIP
                        JOIN REPORTING_MY_TIME_MOVEMENT_BLOBS B ON B.MovementId = MIP.MovementId
                        WHERE MIP.ConsultantId = CD.ConsultantId
                          AND MIP.ProjectId    = P.ProjectId
                        FOR JSON PATH
                    )), '[]') AS ReportAttachments,
                    PI.BlobUrl AS ProfileImage,
                    ROW_NUMBER() OVER (PARTITION BY CONCAT(U.Name, ' ', U.LastName), P.Name ORDER BY P.Name) AS RowNumber
                FROM ActiveConsultants FC
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
                LEFT JOIN AccessToTrackingToolData ATTD ON PCA.ProjectConsultantAssignedId = ATTD.ProjectConsultantAssignedId
                LEFT JOIN AccountsPayableStatus APS ON CD.ConsultantId = APS.ConsultantId
                LEFT JOIN ProfileImages PI ON PI.EntityId = CD.UserId AND PI.rn = 1
                WHERE
                    (NOT EXISTS (SELECT 1 FROM @ProjectIds)
                    OR P.ProjectId IN (SELECT ConsultantProjectId FROM @ProjectIds)) AND
                    (@TransactionStatusName IS NULL OR
                    ((@TransactionStatusName = 'Approved' AND ATTD.AccessToTrackingTool = 0 AND RS.TransactionStatusId IS NULL)
                    OR (@TransactionStatusName = 'Pending' AND ATTD.AccessToTrackingTool <> 0 AND TS.Name IS NULL)
                    OR TS.Name = @TransactionStatusName))
                    AND
                    (@SearchText IS NULL
                        OR LOWER(U.Name) LIKE '%' + LOWER(@SearchText) + '%'
                        OR LOWER(U.LastName) LIKE '%' + LOWER(@SearchText) + '%'
                        OR LOWER(CONCAT(U.Name, ' ', U.LastName)) LIKE '%' + LOWER(@SearchText) + '%') AND
                    (@PaymentPeriod IS NULL OR CD.PaymentPeriod = @PaymentPeriod)
            		AND (@AccountsPayableStatusName IS NULL OR APS.PaymentStatus = @AccountsPayableStatusName OR
                    (@AccountsPayableStatusName = 'Pending' AND APS.PaymentStatus IS NULL))  -- Add the filter by PaymentStatus
                    AND NOT EXISTS (
                        -- Exclude consultants with disabled tracking
                        SELECT 1
                        FROM PROJECTS_CONSULTANTS_PERIODS_DISABLED_TRACKINGS PCPDT
                        WHERE PCPDT.ProjectId = P.ProjectId
                        AND PCPDT.ConsultantId = CD.ConsultantId
                        AND PCPDT.StartPeriodDate >= @StartDate
                        AND PCPDT.EndPeriodDate <= @EndDate
                        AND PCPDT.EndPeriodDate = @EndDate
                    )
            ),
            -- Project-less rows: one row per project-less consultant ('—', hours 0).
            ProjectLessPagedResults AS (
                SELECT
                    CONCAT(U.Name, ' ', U.LastName) AS ConsultantName,
                    CD.ConsultantId,
                    CAST(NULL AS INT) AS ProjectId,
                    N'—' AS ProjectName,
                    CAST(NULL AS INT) AS SubmissionId,
                    'Approved' AS TransactionStatusName,
                    CAST(NULL AS DATETIME2) AS SubmissionDate,
                    CAST(NULL AS DATETIME2) AS LastSubmissionDate,
                    0 AS NumApprovedSubmissions,
                    0 AS NumProjectsIsActive,
                    CAST(0 AS BIT) AS AccessToTrackingTool,
                    COALESCE(APS.PaymentStatus, 'Pending') AS PaymentStatus,
                    CAST('[]' AS NVARCHAR(MAX)) AS HoursReported,
                    CAST('[]' AS NVARCHAR(MAX)) AS ReportAttachments,
                    PI.BlobUrl AS ProfileImage
                FROM ProjectLessConsultants PLC
                JOIN CONSULTANT_DETAILS CD ON PLC.ConsultantId = CD.ConsultantId
                JOIN Users U ON CD.UserId = U.Id
                LEFT JOIN AccountsPayableStatus APS ON CD.ConsultantId = APS.ConsultantId
                LEFT JOIN ProfileImages PI ON PI.EntityId = CD.UserId AND PI.rn = 1
                WHERE
                    -- Drop project-less rows when a project filter is applied
                    NOT EXISTS (SELECT 1 FROM @ProjectIds)
                    AND (@TransactionStatusName IS NULL OR @TransactionStatusName = 'Approved')
                    AND (@SearchText IS NULL
                        OR LOWER(U.Name) LIKE '%' + LOWER(@SearchText) + '%'
                        OR LOWER(U.LastName) LIKE '%' + LOWER(@SearchText) + '%'
                        OR LOWER(CONCAT(U.Name, ' ', U.LastName)) LIKE '%' + LOWER(@SearchText) + '%')
                    AND (@PaymentPeriod IS NULL OR CD.PaymentPeriod = @PaymentPeriod)
                    AND (@AccountsPayableStatusName IS NULL OR COALESCE(APS.PaymentStatus, 'Pending') = @AccountsPayableStatusName OR
                        (@AccountsPayableStatusName = 'Pending' AND APS.PaymentStatus IS NULL))
            )

            -- Store results in a temporary table for further processing
            SELECT *
            INTO #TempPagedResults
            FROM (
                SELECT
                    ConsultantName, ConsultantId, ProjectId, ProjectName, SubmissionId, TransactionStatusName,
                    SubmissionDate, LastSubmissionDate, NumApprovedSubmissions, NumProjectsIsActive,
                    AccessToTrackingTool, PaymentStatus, HoursReported, ReportAttachments, ProfileImage
                FROM PagedResults
                WHERE RowNumber = 1
                UNION ALL
                SELECT
                    ConsultantName, ConsultantId, ProjectId, ProjectName, SubmissionId, TransactionStatusName,
                    SubmissionDate, LastSubmissionDate, NumApprovedSubmissions, NumProjectsIsActive,
                    AccessToTrackingTool, PaymentStatus, HoursReported, ReportAttachments, ProfileImage
                FROM ProjectLessPagedResults
            ) AS Combined;

            -- Get the total count for pagination
            SELECT @TotalCount = COUNT(*) FROM #TempPagedResults;

            -- Fetch the paginated results
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
                AccessToTrackingTool,
                PaymentStatus,
                REPLACE(ReportAttachments, '\/', '/') AS ReportAttachments,
                REPLACE(HoursReported, '\/', '/')      AS HoursReported,
                ProfileImage
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
            	CASE WHEN @FieldToOrder = 'PaymentStatus' AND @DirectionOrder = 'ASC' THEN PaymentStatus END ASC,
                CASE WHEN @FieldToOrder = 'PaymentStatus' AND @DirectionOrder = 'DESC' THEN PaymentStatus END DESC,
                ConsultantName
            OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;

            -- Drop the temporary table
            DROP TABLE #TempPagedResults;
            END";

            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS SP_PAYMENT_SHEETS_GetAllConsultantsToPayWithFilters");
            migrationBuilder.Sql(sp);
        }
    }
}
