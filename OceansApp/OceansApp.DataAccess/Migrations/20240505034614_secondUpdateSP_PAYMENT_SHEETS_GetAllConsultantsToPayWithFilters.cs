using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class secondUpdateSP_PAYMENT_SHEETS_GetAllConsultantsToPayWithFilters : Migration
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
            DECLARE @StartDateFormatted DATE,
            @EndDateFormatted DATE;
            SET @StartDateFormatted = CONVERT(DATE, @StartDate);
            SET @EndDateFormatted = CONVERT(DATE, @EndDate);
            -- Count total results
            SELECT @TotalCount = COUNT(*)
            FROM (
                SELECT
                      CONCAT(U.Name, ' ', U.LastName) AS ConsultantName
                      ,P.ProjectId
                      ,P.Name AS ProjectName
                      ,RS.SubmissionId
                      ,TS.Name AS TransactionStatusName
                      ,RS.SubmissionDate
                      ,RS.LastSubmissionDate
                  FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH
                  JOIN PROJECTS_CONSULTANTS_ASSIGNED PCA ON PCAH.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
                  JOIN PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS HA ON PCAH.ActionId = HA.ActionId
                  JOIN PROJECTS P ON PCA.ProjectId = P.ProjectId
                  JOIN CONSULTANT_DETAILS CD ON PCA.ConsultantId = CD.ConsultantId
                  LEFT JOIN REPORTING_MY_TIME_MOVEMENTS_SUBMISSIONS RS ON PCA.ConsultantId = RS.ConsultantId
                  AND P.ProjectId = RS.ProjectId AND CONVERT(DATE, RS.StartPeriodDate) = @StartDateFormatted
                  AND CONVERT(DATE, RS.EndPeriodDate) = @EndDateFormatted
                  LEFT JOIN TRANSACTION_STATUSES TS ON RS.TransactionStatusId = TS.TransactionStatusId
                  JOIN Users U ON CD.UserId = U.Id
                WHERE 
                (@ProjectId IS NULL OR P.ProjectId = @ProjectId) AND
                (@TransactionStatusId IS NULL OR (TS.TransactionStatusId = @TransactionStatusId AND TS.TransactionStatusId IS NOT NULL))
                AND (@SearchText IS NULL 
                            OR LOWER(U.Name) LIKE '%' + LOWER(@SearchText) + '%'
                            OR LOWER(U.LastName) LIKE '%' + LOWER(@SearchText) + '%'
                            OR LOWER(CONCAT(U.Name, ' ', U.LastName)) LIKE '%' + LOWER(@SearchText) + '%')
                AND (@PaymentPeriod IS NULL OR CD.PaymentPeriod = @PaymentPeriod)
                AND (
                    EXISTS (
                        SELECT 1
                        FROM (
                            SELECT TOP(1)
                                CASE
                                    WHEN HA.Name = 'Consultant Activated' THEN 'Active'
                                    WHEN HA.Name = 'Consultant Assigned First Time' THEN 'Assigned First Time'
                                    ELSE 'No actions'
                                END AS ActionOutcome
                            FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH
                            INNER JOIN PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS HA ON PCAH.ActionId = HA.ActionId
                            WHERE PCAH.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
                            AND CONVERT(DATE, PCAH.ActionDate) <= @EndDateFormatted
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
                            AND CONVERT(DATE, PCAH.ActionDate) >= @StartDateFormatted 
                            AND CONVERT(DATE, PCAH.ActionDate) <= @EndDateFormatted
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
                            AND CONVERT(DATE, PCAH.ActionDate) < @StartDateFormatted 
                            ORDER BY PCAH.ActionDate DESC, PCAH.Id DESC
                        ) SubQuery
                        WHERE SubQuery.ActionOutcome = 'Inactive'
                    )
                )
                GROUP BY CONCAT(U.Name, ' ', U.LastName), P.ProjectId, P.Name, RS.SubmissionId, 
                TS.Name, RS.SubmissionDate, RS.LastSubmissionDate
            ) AS TotalRegisters;
            
                        -- Request with pagination
                        SELECT
                  CONCAT(U.Name, ' ', U.LastName) AS ConsultantName
            	  ,P.ProjectId
                  ,P.Name AS ProjectName
            	  ,RS.SubmissionId
            	  ,TS.Name AS TransactionStatusName
            	  ,RS.SubmissionDate
            	  ,RS.LastSubmissionDate
              FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH
              JOIN PROJECTS_CONSULTANTS_ASSIGNED PCA ON PCAH.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
              JOIN PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS HA ON PCAH.ActionId = HA.ActionId
              JOIN PROJECTS P ON PCA.ProjectId = P.ProjectId
              JOIN CONSULTANT_DETAILS CD ON PCA.ConsultantId = CD.ConsultantId
              LEFT JOIN REPORTING_MY_TIME_MOVEMENTS_SUBMISSIONS RS ON PCA.ConsultantId = RS.ConsultantId
              AND P.ProjectId = RS.ProjectId AND CONVERT(DATE, RS.StartPeriodDate) = @StartDateFormatted
              AND CONVERT(DATE, RS.EndPeriodDate) = @EndDateFormatted
              LEFT JOIN TRANSACTION_STATUSES TS ON RS.TransactionStatusId = TS.TransactionStatusId
              JOIN Users U ON CD.UserId = U.Id
            WHERE 
            (@ProjectId IS NULL OR P.ProjectId = @ProjectId) AND
            (@TransactionStatusId IS NULL OR (TS.TransactionStatusId = @TransactionStatusId AND TS.TransactionStatusId IS NOT NULL))
            AND (@SearchText IS NULL 
                        OR LOWER(U.Name) LIKE '%' + LOWER(@SearchText) + '%'
                  			OR LOWER(U.LastName) LIKE '%' + LOWER(@SearchText) + '%'
                  			OR LOWER(CONCAT(U.Name, ' ', U.LastName)) LIKE '%' + LOWER(@SearchText) + '%')
            AND (@PaymentPeriod IS NULL OR CD.PaymentPeriod = @PaymentPeriod)
            AND (
                EXISTS (
                    SELECT 1
                    FROM (
                        SELECT TOP(1)
                            CASE
                                WHEN HA.Name = 'Consultant Activated' THEN 'Active'
                                WHEN HA.Name = 'Consultant Assigned First Time' THEN 'Assigned First Time'
                                ELSE 'No actions'
                            END AS ActionOutcome
                        FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH
                        INNER JOIN PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS HA ON PCAH.ActionId = HA.ActionId
                        WHERE PCAH.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
                        AND CONVERT(DATE, PCAH.ActionDate) <= @EndDateFormatted
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
                        AND CONVERT(DATE, PCAH.ActionDate) >= @StartDateFormatted 
                        AND CONVERT(DATE, PCAH.ActionDate) <= @EndDateFormatted
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
                        AND CONVERT(DATE, PCAH.ActionDate) < @StartDateFormatted 
                        ORDER BY PCAH.ActionDate DESC, PCAH.Id DESC
                    ) SubQuery
                    WHERE SubQuery.ActionOutcome = 'Inactive'
                )
            )
            GROUP BY CONCAT(U.Name, ' ', U.LastName), P.ProjectId, P.Name, RS.SubmissionId, 
            TS.Name, RS.SubmissionDate, RS.LastSubmissionDate
                     
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
            @TotalCount INT OUTPUT,
            @StartDateFormatted DATE,
            @EndDateFormatted DATE
            AS
            BEGIN
            SET @StartDateFormatted = CONVERT(DATE, @StartDate);
            SET @EndDateFormatted = CONVERT(DATE, @EndDate);
            -- Count total results
            SELECT @TotalCount = COUNT(*)
            FROM (
                SELECT
                      CONCAT(U.Name, ' ', U.LastName) AS ConsultantName
                      ,P.ProjectId
                      ,P.Name AS ProjectName
                      ,RS.SubmissionId
                      ,TS.Name AS TransactionStatusName
                      ,RS.SubmissionDate
                      ,RS.LastSubmissionDate
                  FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH
                  JOIN PROJECTS_CONSULTANTS_ASSIGNED PCA ON PCAH.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
                  JOIN PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS HA ON PCAH.ActionId = HA.ActionId
                  JOIN PROJECTS P ON PCA.ProjectId = P.ProjectId
                  JOIN CONSULTANT_DETAILS CD ON PCA.ConsultantId = CD.ConsultantId
                  LEFT JOIN REPORTING_MY_TIME_MOVEMENTS_SUBMISSIONS RS ON PCA.ConsultantId = RS.ConsultantId
                  AND P.ProjectId = RS.ProjectId AND CONVERT(DATE, RS.StartPeriodDate) = @StartDateFormatted
                  AND CONVERT(DATE, RS.EndPeriodDate) = @EndDateFormatted
                  LEFT JOIN TRANSACTION_STATUSES TS ON RS.TransactionStatusId = TS.TransactionStatusId
                  JOIN Users U ON CD.UserId = U.Id
                WHERE 
                (@ProjectId IS NULL OR P.ProjectId = @ProjectId) AND
                (@TransactionStatusId IS NULL OR (TS.TransactionStatusId = @TransactionStatusId AND TS.TransactionStatusId IS NOT NULL))
                AND (@SearchText IS NULL 
                            OR LOWER(U.Name) LIKE '%' + LOWER(@SearchText) + '%'
                            OR LOWER(U.LastName) LIKE '%' + LOWER(@SearchText) + '%'
                            OR LOWER(CONCAT(U.Name, ' ', U.LastName)) LIKE '%' + LOWER(@SearchText) + '%')
                AND (@PaymentPeriod IS NULL OR CD.PaymentPeriod = @PaymentPeriod)
                AND (
                    EXISTS (
                        SELECT 1
                        FROM (
                            SELECT TOP(1)
                                CASE
                                    WHEN HA.Name = 'Consultant Activated' THEN 'Active'
                                    WHEN HA.Name = 'Consultant Assigned First Time' THEN 'Assigned First Time'
                                    ELSE 'No actions'
                                END AS ActionOutcome
                            FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH
                            INNER JOIN PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS HA ON PCAH.ActionId = HA.ActionId
                            WHERE PCAH.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
                            AND CONVERT(DATE, PCAH.ActionDate) <= @EndDateFormatted
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
                            AND CONVERT(DATE, PCAH.ActionDate) >= @StartDateFormatted 
                            AND CONVERT(DATE, PCAH.ActionDate) <= @EndDateFormatted
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
                            AND CONVERT(DATE, PCAH.ActionDate) < @StartDateFormatted 
                            ORDER BY PCAH.ActionDate DESC, PCAH.Id DESC
                        ) SubQuery
                        WHERE SubQuery.ActionOutcome = 'Inactive'
                    )
                )
                GROUP BY CONCAT(U.Name, ' ', U.LastName), P.ProjectId, P.Name, RS.SubmissionId, 
                TS.Name, RS.SubmissionDate, RS.LastSubmissionDate
            ) AS TotalRegisters;
            
                        -- Request with pagination
                        SELECT
                  CONCAT(U.Name, ' ', U.LastName) AS ConsultantName
            	  ,P.ProjectId
                  ,P.Name AS ProjectName
            	  ,RS.SubmissionId
            	  ,TS.Name AS TransactionStatusName
            	  ,RS.SubmissionDate
            	  ,RS.LastSubmissionDate
              FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH
              JOIN PROJECTS_CONSULTANTS_ASSIGNED PCA ON PCAH.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
              JOIN PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS HA ON PCAH.ActionId = HA.ActionId
              JOIN PROJECTS P ON PCA.ProjectId = P.ProjectId
              JOIN CONSULTANT_DETAILS CD ON PCA.ConsultantId = CD.ConsultantId
              LEFT JOIN REPORTING_MY_TIME_MOVEMENTS_SUBMISSIONS RS ON PCA.ConsultantId = RS.ConsultantId
              AND P.ProjectId = RS.ProjectId AND CONVERT(DATE, RS.StartPeriodDate) = @StartDateFormatted
              AND CONVERT(DATE, RS.EndPeriodDate) = @EndDateFormatted
              LEFT JOIN TRANSACTION_STATUSES TS ON RS.TransactionStatusId = TS.TransactionStatusId
              JOIN Users U ON CD.UserId = U.Id
            WHERE 
            (@ProjectId IS NULL OR P.ProjectId = @ProjectId) AND
            (@TransactionStatusId IS NULL OR (TS.TransactionStatusId = @TransactionStatusId AND TS.TransactionStatusId IS NOT NULL))
            AND (@SearchText IS NULL 
                        OR LOWER(U.Name) LIKE '%' + LOWER(@SearchText) + '%'
                  			OR LOWER(U.LastName) LIKE '%' + LOWER(@SearchText) + '%'
                  			OR LOWER(CONCAT(U.Name, ' ', U.LastName)) LIKE '%' + LOWER(@SearchText) + '%')
            AND (@PaymentPeriod IS NULL OR CD.PaymentPeriod = @PaymentPeriod)
            AND (
                EXISTS (
                    SELECT 1
                    FROM (
                        SELECT TOP(1)
                            CASE
                                WHEN HA.Name = 'Consultant Activated' THEN 'Active'
                                WHEN HA.Name = 'Consultant Assigned First Time' THEN 'Assigned First Time'
                                ELSE 'No actions'
                            END AS ActionOutcome
                        FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH
                        INNER JOIN PROJECTS_CONSULTANTS_ASSIGNED_HISTORY_ACTIONS HA ON PCAH.ActionId = HA.ActionId
                        WHERE PCAH.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
                        AND CONVERT(DATE, PCAH.ActionDate) <= @EndDateFormatted
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
                        AND CONVERT(DATE, PCAH.ActionDate) >= @StartDateFormatted 
                        AND CONVERT(DATE, PCAH.ActionDate) <= @EndDateFormatted
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
                        AND CONVERT(DATE, PCAH.ActionDate) < @StartDateFormatted 
                        ORDER BY PCAH.ActionDate DESC, PCAH.Id DESC
                    ) SubQuery
                    WHERE SubQuery.ActionOutcome = 'Inactive'
                )
            )
            GROUP BY CONCAT(U.Name, ' ', U.LastName), P.ProjectId, P.Name, RS.SubmissionId, 
            TS.Name, RS.SubmissionDate, RS.LastSubmissionDate
                     
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
