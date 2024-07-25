using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OceansApp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class sevenUpdateSP_PROJECTS_CONSULTANTS_ASSIGNED_GetConsultantSelectedProjectInfo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //UPDATE STORED PROCEDURE
            var sp = @"CREATE PROCEDURE SP_PROJECTS_CONSULTANTS_ASSIGNED_GetConsultantSelectedProjectInfo
            @UserId NVARCHAR(450),
            @StartDate DATE,
            @EndDate DATE
            AS
            BEGIN
            -- CTE to count the number of projects assigned to each consultant
            WITH ConsultantProjectCount AS (
                SELECT 
                    ConsultantId,
                    COUNT(*) AS NumAssignedProjects
                FROM PROJECTS_CONSULTANTS_ASSIGNED
                GROUP BY ConsultantId
            ),
            -- CTE to determine the IsActive status
            IsActiveStatus AS (
                SELECT
                    PCA.ProjectConsultantAssignedId,
                    CASE
                        -- Check the latest record before @StartDate
                        WHEN (
                            SELECT TOP 1 PCAH.IsActive
                            FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH
                            WHERE PCAH.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
                            AND PCAH.ActionDate < @StartDate
                            ORDER BY PCAH.ActionDate DESC, PCAH.Id DESC
                        ) = 1 THEN 1
                        -- If no record before @StartDate, check records between @StartDate and @EndDate with IsActive = 1
                        WHEN EXISTS (
                            SELECT 1
                            FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH
                            WHERE PCAH.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
                            AND PCAH.ActionDate >= @StartDate
                            AND PCAH.ActionDate <= @EndDate
                            AND PCAH.IsActive = 1
                        ) THEN 1
                        -- Default case
                        ELSE 0
                    END AS IsActive
                FROM PROJECTS_CONSULTANTS_ASSIGNED PCA
            ),
            -- CTE to determine the AccessToTrackingTool status
            AccessToTrackingToolStatus AS (
                SELECT
                    PCA.ProjectConsultantAssignedId,
                    CASE
                        -- Check the latest record before @StartDate
                        WHEN (
                            SELECT TOP 1 PCAH.AccessToTrackingTool
                            FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH
                            WHERE PCAH.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
                            AND PCAH.ActionDate < @StartDate 
                            ORDER BY PCAH.ActionDate DESC, PCAH.Id DESC
                        ) = 1 THEN 1
                        -- If no record before @StartDate, check records between @StartDate and @EndDate with AccessToTrackingTool = 1
                        WHEN EXISTS (
                            SELECT 1
                            FROM PROJECTS_CONSULTANTS_ASSIGNED_HISTORY PCAH
                            WHERE PCAH.ProjectConsultantAssignedId = PCA.ProjectConsultantAssignedId
                            AND PCAH.ActionDate >= @StartDate
                            AND PCAH.ActionDate <= @EndDate
                            AND PCAH.AccessToTrackingTool = 1
                        ) THEN 1
                        -- Default case
                        ELSE 0
                    END AS AccessToTrackingTool
                FROM PROJECTS_CONSULTANTS_ASSIGNED PCA
            )
            
            SELECT 
                CD.PaymentPeriod,
                PUS.ProjectId,
                P.Name AS ProjectName,
                P.ClientHasTrackingTool,
                SMU.Name + ' ' + SMU.LastName AS SuccessManagerName,
                SMU.Email AS SuccessManagerEmail,
                CD.ParticipatesInOnCalls,
            	ATTS.AccessToTrackingTool,
                CPC.NumAssignedProjects,
                IAS.IsActive
            FROM CONSULTANT_DETAILS CD
            LEFT JOIN PROJECTS_USERS_SELECTED PUS ON CD.UserId = PUS.UserId
            LEFT JOIN PROJECTS P ON PUS.ProjectId = P.ProjectId
            INNER JOIN CONSULTANT_DETAILS SM ON P.SuccessManagerId = SM.ConsultantId
            INNER JOIN Users SMU ON SM.UserId = SMU.Id
            INNER JOIN PROJECTS_CONSULTANTS_ASSIGNED PCA ON PUS.ProjectId = PCA.ProjectId AND PCA.ConsultantId = CD.ConsultantId
            LEFT JOIN ConsultantProjectCount CPC ON CD.ConsultantId = CPC.ConsultantId
            LEFT JOIN IsActiveStatus IAS ON PCA.ProjectConsultantAssignedId = IAS.ProjectConsultantAssignedId
            LEFT JOIN AccessToTrackingToolStatus ATTS ON PCA.ProjectConsultantAssignedId = ATTS.ProjectConsultantAssignedId
            WHERE CD.UserId = @UserId;
            END";

            // Delete stored procedure if exists
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS SP_PROJECTS_CONSULTANTS_ASSIGNED_GetConsultantSelectedProjectInfo");

            // Create new stored Procedure
            migrationBuilder.Sql(sp);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // restart the stored procedure to the original version
            var spOriginal = @"CREATE PROCEDURE SP_PROJECTS_CONSULTANTS_ASSIGNED_GetConsultantSelectedProjectInfo
            @UserId NVARCHAR(450)
            AS
            BEGIN
            WITH ConsultantProjectCount AS (
                SELECT 
                    ConsultantId,
                    COUNT(*) AS NumAssignedProjects
                FROM PROJECTS_CONSULTANTS_ASSIGNED
                GROUP BY ConsultantId
            )
            SELECT 
            CD.PaymentPeriod,
                PUS.ProjectId,
                P.Name AS ProjectName,
                P.ClientHasTrackingTool,
                SMU.Name + ' ' + SMU.LastName AS SucessManagerName,
                SMU.Email AS SuccessManagerEmail,
                CD.ParticipatesInOnCalls,
                PCA.AccessToTrackingTool,
                CPC.NumAssignedProjects
            FROM CONSULTANT_DETAILS CD
            LEFT JOIN PROJECTS_USERS_SELECTED PUS ON CD.UserId = PUS.UserId
            LEFT JOIN PROJECTS P ON PUS.ProjectId = P.ProjectId
            INNER JOIN CONSULTANT_DETAILS SM ON P.SuccessManagerId = SM.ConsultantId
            INNER JOIN Users SMU ON SM.UserId = SMU.Id
            INNER JOIN PROJECTS_CONSULTANTS_ASSIGNED PCA ON PUS.ProjectId = PCA.ProjectId AND PCA.ConsultantId = CD.ConsultantId
            LEFT JOIN ConsultantProjectCount CPC ON CD.ConsultantId = CPC.ConsultantId
            WHERE CD.UserId = @UserId;
            END";

            migrationBuilder.Sql("DROP PROCEDURE IF SP_PROJECTS_CONSULTANTS_ASSIGNED_GetConsultantSelectedProjectInfo");
            migrationBuilder.Sql(spOriginal);
        }
    }
}
