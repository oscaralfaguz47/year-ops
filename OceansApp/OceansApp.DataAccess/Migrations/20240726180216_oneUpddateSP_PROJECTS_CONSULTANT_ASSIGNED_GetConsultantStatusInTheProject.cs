using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class oneUpddateSP_PROJECTS_CONSULTANT_ASSIGNED_GetConsultantStatusInTheProject : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //UPDATE STORED PROCEDURE
            var sp = @"CREATE PROCEDURE SP_PROJECTS_CONSULTANT_ASSIGNED_GetConsultantStatusInTheProject
            @UserId NVARCHAR(450),
            @StartDate DATE,
            @EndDate DATE
            AS
            BEGIN
            -- CTE to determine IsActive status
            WITH IsActiveStatus AS (
                SELECT
                    PCA.ProjectConsultantAssignedId,
                    CASE
                        -- Check most recent record before @StartDate
                        WHEN (
                            SELECT TOP 1 PCAH.IsActive
                            FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH
                            WHERE PCAH.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
                            AND PCAH.ActionDate < @StartDate
                            ORDER BY PCAH.ActionDate DESC, PCAH.Id DESC
                        ) = 1 THEN 1
                        -- If there are no records before @StartDate, check records between @StartDate and @EndDate with IsActive = 1
                        WHEN EXISTS (
                            SELECT 1
                            FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH
                            WHERE PCAH.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
                            AND PCAH.ActionDate >= @StartDate
                            AND PCAH.ActionDate <= @EndDate
                            AND PCAH.IsActive = 1
                        ) THEN 1
                        ELSE 0
                    END AS IsActive
                FROM PROJECTS_CONSULTANTS_ASSIGNED PCA
            ),
            -- CTE to determine the status of AccessToTrackingTool
            AccessToTrackingToolStatus AS (
                SELECT
                    PCA.ProjectConsultantAssignedId,
                    ISNULL((
                        SELECT TOP 1 PCAH.AccessToTrackingTool
                        FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH
                        WHERE PCAH.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
                        AND PCAH.ActionDate <= @EndDate
                        ORDER BY PCAH.ActionDate DESC, PCAH.Id DESC
                    ), 0) AS AccessToTrackingTool
                FROM PROJECTS_CONSULTANTS_ASSIGNED PCA
            )
            SELECT 
                ISNULL(ATTS.AccessToTrackingTool, 0) AS AccessToTrackingTool,
                ISNULL(IAS.IsActive, 0) AS IsActive
            FROM CONSULTANT_DETAILS CD
            LEFT JOIN PROJECTS_USERS_SELECTED PUS ON CD.UserId = PUS.UserId
            LEFT JOIN PROJECTS_CONSULTANTS_ASSIGNED PCA ON PUS.ProjectId = PCA.ProjectId AND PCA.ConsultantId = CD.ConsultantId
            LEFT JOIN IsActiveStatus IAS ON PCA.ProjectConsultantAssignedId = IAS.ProjectConsultantAssignedId
            LEFT JOIN AccessToTrackingToolStatus ATTS ON PCA.ProjectConsultantAssignedId = ATTS.ProjectConsultantAssignedId
            WHERE CD.UserId = @UserId;
            END";

            // Delete stored procedure if exists
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS SP_PROJECTS_CONSULTANT_ASSIGNED_GetConsultantStatusInTheProject");

            // Create new stored Procedure
            migrationBuilder.Sql(sp);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // restart the stored procedure to the original version
            var spOriginal = @"CREATE PROCEDURE SP_PROJECTS_CONSULTANT_ASSIGNED_GetConsultantStatusInTheProject
            @UserId NVARCHAR(450),
            @StartDate DATE,
            @EndDate DATE
            AS
            BEGIN
            -- CTE to determine IsActive status
            WITH IsActiveStatus AS (
                SELECT
                    PCA.ProjectConsultantAssignedId,
                    CASE
                        -- Check most recent record before @StartDate
                        WHEN (
                            SELECT TOP 1 PCAH.IsActive
                            FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH
                            WHERE PCAH.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
                            AND PCAH.ActionDate < @StartDate
                            ORDER BY PCAH.ActionDate DESC, PCAH.Id DESC
                        ) = 1 THEN 1
                        -- If there are no records before @StartDate, check records between @StartDate and @EndDate with IsActive = 1
                        WHEN EXISTS (
                            SELECT 1
                            FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH
                            WHERE PCAH.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
                            AND PCAH.ActionDate >= @StartDate
                            AND PCAH.ActionDate <= @EndDate
                            AND PCAH.IsActive = 1
                        ) THEN 1
                        ELSE 0
                    END AS IsActive
                FROM PROJECTS_CONSULTANTS_ASSIGNED PCA
            ),
            -- CTE to determine the status of AccessToTrackingTool
            AccessToTrackingToolStatus AS (
                SELECT
                    PCA.ProjectConsultantAssignedId,
                    CASE
                        -- Check most recent record before @StartDate
                        WHEN (
                            SELECT TOP 1 PCAH.AccessToTrackingTool
                            FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH
                            WHERE PCAH.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
                            AND PCAH.ActionDate < @StartDate 
                            ORDER BY PCAH.ActionDate DESC, PCAH.Id DESC
                        ) = 1 THEN 1
                        -- If there are no records before @StartDate, check records between @StartDate and @EndDate with AccessToTrackingTool = 1
                        WHEN EXISTS (
                            SELECT 1
                            FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH
                            WHERE PCAH.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
                            AND PCAH.ActionDate >= @StartDate
                            AND PCAH.ActionDate <= @EndDate
                            AND PCAH.AccessToTrackingTool = 1
                        ) THEN 1
                        ELSE 0
                    END AS AccessToTrackingTool
                FROM PROJECTS_CONSULTANTS_ASSIGNED PCA
            )
            
            SELECT 
                ATTS.AccessToTrackingTool,
                IAS.IsActive
            FROM CONSULTANT_DETAILS CD
            LEFT JOIN PROJECTS_USERS_SELECTED PUS ON CD.UserId = PUS.UserId
            LEFT JOIN PROJECTS_CONSULTANTS_ASSIGNED PCA ON PUS.ProjectId = PCA.ProjectId AND PCA.ConsultantId = CD.ConsultantId
            LEFT JOIN IsActiveStatus IAS ON PCA.ProjectConsultantAssignedId = IAS.ProjectConsultantAssignedId
            LEFT JOIN AccessToTrackingToolStatus ATTS ON PCA.ProjectConsultantAssignedId = ATTS.ProjectConsultantAssignedId
            WHERE CD.UserId = @UserId;
            END";

            migrationBuilder.Sql("DROP PROCEDURE IF SP_PROJECTS_CONSULTANT_ASSIGNED_GetConsultantStatusInTheProject");
            migrationBuilder.Sql(spOriginal);
        }
    }
}
